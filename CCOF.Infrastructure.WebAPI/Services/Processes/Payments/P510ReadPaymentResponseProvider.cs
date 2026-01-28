using HandlebarsDotNet;
using Microsoft.Extensions.Options;
using CCOF.Infrastructure.WebAPI.Extensions;
using CCOF.Infrastructure.WebAPI.Messages;
using CCOF.Infrastructure.WebAPI.Models;
using CCOF.Infrastructure.WebAPI.Services.AppUsers;
using CCOF.Infrastructure.WebAPI.Services.D365WebApi;
using System.Net;
using System.Text.Json.Nodes;
using FixedWidthParserWriter;
using CCOF.Core.DataContext;
using System.Text.Json;
using CCOF.Infrastructure.WebAPI.Models;

namespace CCOF.Infrastructure.WebAPI.Services.Processes.Payments;

public class P510ReadPaymentResponseProvider(IOptionsSnapshot<ExternalServices> bccasApiSettings, ID365AppUserService appUserService, ID365WebApiService d365WebApiService, ILoggerFactory loggerFactory, TimeProvider timeProvider) : ID365ProcessProvider
{
    private readonly BCCASApi _BCCASApi = bccasApiSettings.Value.BCCASApi;
    private readonly ID365AppUserService _appUserService = appUserService;
    private readonly ID365WebApiService _d365webapiservice = d365WebApiService;
    private readonly ILogger _logger = loggerFactory.CreateLogger(LogCategory.Process);
    private readonly TimeProvider _timeProvider = timeProvider;
    private ProcessData? _data;
    private ProcessParameter? _processParams;

    public Int16 ProcessId => Setup.Process.Payments.GetPaymentResponseId;
    public string ProcessName => Setup.Process.Payments.GetPaymentResponseName;
    
    public string RequestUri
    {
        get
        {
            // this query is just for info,payment file data is coming from requesturl
            var fetchXml = $"""
                    <fetch version="1.0" output-format="xml-platform" mapping="logical" distinct="false">
                      <entity name="ofm_payment_file_exchange">
                        <attribute name="ofm_payment_file_exchangeid" />
                        <attribute name="ofm_name" />
                        <attribute name="createdon" />
                        <order attribute="ofm_name" descending="false" />
                        <filter type="and">    
                          <condition attribute="ofm_payment_file_exchangeid" operator="eq" value="{_processParams?.PaymentFile?.paymentfileId}" />
                        </filter>
                      </entity>
                    </fetch>
                    """;

            var requestUri = $"""
                         ofm_payment_file_exchanges({_processParams?.PaymentFile?.paymentfileId})/ofm_feedback_document_memo
                         """;

            return requestUri;
        }
    }
    
    public string RequestInvoiceUri
    {
        get
        {
            var localDateOnlyPST = DateTime.UtcNow.ToLocalPST().Date;

            // For reference only
            var fetchXml = $"""
                    <fetch>
                      <entity name="ccof_invoice">
                        <attribute name="ccof_batch_number" />
                        <attribute name="ccof_effective_date" />
                        <attribute name="ccof_grand_total" />
                        <attribute name="ccof_invoice_date" />
                        <attribute name="ccof_invoice_number" />
                        <attribute name="ccof_invoice_received_date" />
                        <attribute name="ccof_invoiceid" />
                        <attribute name="ccof_name" />
                        <attribute name="ccof_organization" />
                        <attribute name="ccof_organization_id" />
                        <attribute name="ccof_payment_type" />
                        <attribute name="ccof_paymentmethod" />
                        <attribute name="ccof_remittancemessage" />
                        <attribute name="ccof_revised_effective_date" />
                        <attribute name="ccof_revised_invoice_date" />
                        <attribute name="ccof_revised_invoice_received_date" />
                        <attribute name="ccof_site_number" />
                        <attribute name="ccof_supplier_number" />
                        <attribute name="createdby" />
                        <attribute name="createdon" />
                        <attribute name="createdonbehalfby" />
                        <attribute name="exchangerate" />
                        <attribute name="modifiedby" />
                        <attribute name="modifiedon" />
                        <attribute name="modifiedonbehalfby" />
                        <attribute name="overriddencreatedon" />
                        <attribute name="ownerid" />
                        <attribute name="owningbusinessunit" />
                        <attribute name="statecode" />
                        <attribute name="statuscode" />
                        <attribute name="transactioncurrencyid" />
                        <filter>
                          <condition attribute="statuscode" operator="eq" value="{(int)CcOf_Invoice_StatusCode.ProcessingPayment}" />
                          <condition attribute="owningbusinessunitname" operator="like" value="%CCOF%" />
                        </filter>
                       
                      </entity>
                    </fetch>
                    """;

            var requestUri = $"""
                         ccof_invoices?fetchXml={WebUtility.UrlEncode(fetchXml)}
                         """;

            return requestUri;

        }
    }
    public string RequestCCOFPaymentLineUri
    {
        get
        {
            var fetchXml = $"""
                    <fetch version="1.0" output-format="xml-platform" mapping="logical" distinct="false">
                      <entity name="ofm_payment">
                        <attribute name="ofm_paymentid" />
                        <attribute name="ofm_name" />
                        <attribute name="ofm_fiscal_year" />
                        <attribute name="ofm_payment_type" />
                        <attribute name="statuscode" />
                        <attribute name="ofm_invoice_number" />
                        <attribute name="ofm_cas_response" />
                        <attribute name="ccof_invoice" />
                        <order attribute="ofm_name" descending="false" />
                     <filter type="and">
                       <condition attribute="owningbusinessunitname" operator="like" value="%CCOF%" />
                       <condition attribute="statuscode" operator="eq" value="{(int)OfM_Payment_StatusCode.ProcessingPayment}" />
                     </filter>
                    </entity>
                    </fetch>
                    """;
            var requestUri = $"""
                         ofm_payments?fetchXml={WebUtility.UrlEncode(fetchXml)}
                         """;

            return requestUri;
        }
    }

    public async Task<ProcessData> GetDataAsync()
    {
        _logger.LogDebug(CustomLogEvent.Process, "Calling GetData of {nameof}", nameof(P510ReadPaymentResponseProvider));

        if (_data is null)
        {
            var response = await _d365webapiservice.SendRetrieveRequestAsync(_appUserService.AZSystemAppUser, RequestUri, isProcess: true);
            if (!response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                _logger.LogError(CustomLogEvent.Process, "Failed to query the requests with the server error {responseBody}", responseBody);

                return await Task.FromResult(new ProcessData(string.Empty));
            }

            var jsonObject = await response.Content.ReadFromJsonAsync<JsonObject>();

            JsonNode d365Result = string.Empty;
            if (jsonObject?.TryGetPropertyValue("value", out var currentValue) == true)
            {

                d365Result = currentValue!;
            }

            _data = new ProcessData(d365Result);

            _logger.LogDebug(CustomLogEvent.Process, "Query Result {_data}", _data.Data.ToJsonString());
        }

        return await Task.FromResult(_data);
    }
    
    public async Task<ProcessData> GetInvoiceLinesAsync()
    {
        _logger.LogDebug(CustomLogEvent.Process, "Calling GetData of {nameof}", nameof(P510ReadPaymentResponseProvider));

        var response = await _d365webapiservice.SendRetrieveRequestAsync(_appUserService.AZSystemAppUser, RequestInvoiceUri, isProcess: true);
        if (!response.IsSuccessStatusCode)
        {
            var responseBody = await response.Content.ReadAsStringAsync();
            _logger.LogError(CustomLogEvent.Process, "Failed to query the requests with the server error {responseBody}", responseBody);

            return await Task.FromResult(new ProcessData(string.Empty));
        }

        var jsonObject = await response.Content.ReadFromJsonAsync<JsonObject>();

        JsonNode d365Result = string.Empty;
        if (jsonObject?.TryGetPropertyValue("value", out var currentValue) == true)
        {
            if (currentValue?.AsArray().Count == 0)
            {
                _logger.LogInformation(CustomLogEvent.Process, "No records found");
            }
            d365Result = currentValue!;


            _data = new ProcessData(d365Result);

            _logger.LogDebug(CustomLogEvent.Process, "Query Result {_data}", _data.Data.ToJsonString());
        }

        return await Task.FromResult(_data);

    }
    public async Task<ProcessData> GetPayLinesAsync()
    {
        _logger.LogDebug(CustomLogEvent.Process, "Calling GetData of {nameof}", nameof(P510ReadPaymentResponseProvider));

        var response = await _d365webapiservice.SendRetrieveRequestAsync(_appUserService.AZSystemAppUser, RequestCCOFPaymentLineUri, isProcess: true);
        if (!response.IsSuccessStatusCode)
        {
            var responseBody = await response.Content.ReadAsStringAsync();
            _logger.LogError(CustomLogEvent.Process, "Failed to query the requests with the server error {responseBody}", responseBody);

            return await Task.FromResult(new ProcessData(string.Empty));
        }

        var jsonObject = await response.Content.ReadFromJsonAsync<JsonObject>();

        JsonNode d365Result = string.Empty;
        if (jsonObject?.TryGetPropertyValue("value", out var currentValue) == true)
        {
            if (currentValue?.AsArray().Count == 0)
            {
                _logger.LogInformation(CustomLogEvent.Process, "No records found");
            }
            d365Result = currentValue!;


            _data = new ProcessData(d365Result);

            _logger.LogDebug(CustomLogEvent.Process, "Query Result {_data}", _data.Data.ToJsonString());
        }

        return await Task.FromResult(_data);

    }

    public async Task<JsonObject> RunProcessAsync(ID365AppUserService appUserService, ID365WebApiService d365WebApiService, ProcessParameter processParams)
    {
        _processParams = processParams;
        List<string> batchfeedback = [];
        List<string> headerfeedback = [];
        List<string> listfeedback = [];
        List<string> linefeedback = [];
        List<FeedbackHeader> headers = [];
        var createIntregrationLogTasks = new List<Task>();
        var startTime = _timeProvider.GetTimestamp();
        
        var localData = await GetDataAsync();
        var downloadfile = Convert.FromBase64String(localData.Data.ToString());

        listfeedback = System.Text.Encoding.UTF8.GetString(downloadfile).Replace("APBG", "APBG#APBG").Split("APBG#").ToList();
        listfeedback.RemoveAll(item => string.IsNullOrWhiteSpace(item));
        batchfeedback = listfeedback.Select(g => g.Replace("APBH", "APBH#APBH")).SelectMany(group => group.Split("APBH#")).ToList();
        headerfeedback = listfeedback.Select(g => g.Replace("APIH", "APIH#APIH")).SelectMany(group => group.Split("APIH#")).ToList();

        foreach (string data in headerfeedback)
        {
            List<FeedbackLine> lines = [];
            linefeedback = data.Split('\n').Where(g => g.StartsWith("APIL")).Select(g => g).ToList();
            foreach (string list1 in linefeedback)
            {
                FeedbackLine line = new CustomFileProvider<FeedbackLine>().Parse(new List<string> { list1.TrimStart() });
                lines.Add(line);

            }
            FeedbackHeader header = new CustomFileProvider<FeedbackHeader>().Parse(new List<string> { data });
            header.feedbackLine = lines;
            headers.Add(header);
        }

        var localInvoiceData = await GetInvoiceLinesAsync();
        var serializedInvoiceData = JsonSerializer.Deserialize<List<CcofInvoice>>(localInvoiceData.Data.ToString());
        var localPayData = await GetPayLinesAsync();
        var serializedPayData = JsonSerializer.Deserialize<List<D365PaymentLine>>(localPayData.Data.ToString());
        

        List<HttpRequestMessage> updateInvoiceRequests = [];
        List<HttpRequestMessage> updatePaymentLineRequests = [];


        serializedInvoiceData?.ForEach(async invoice =>
        {
            var line = headers.SelectMany(p => p.feedbackLine).FirstOrDefault(pl => pl.ILInvoice == invoice.ccof_invoice_number && pl.ILDescription.Contains(invoice?.ccof_payment_type.ToString()));
            var header = headers.Where(p => p.IHInvoice == invoice.ccof_invoice_number).FirstOrDefault();
            var localDateOnlyPST = DateTime.UtcNow.ToLocalPST().Date;

            DateTime revisedInvoiceDate = localDateOnlyPST;
            DateTime revisedInvoiceReceivedDate = localDateOnlyPST;
            DateTime revisedEffectiveDate = localDateOnlyPST;

            if (line != null && header != null)
            {
                string casResponse = (line?.ILCode != "0000") ? string.Concat("Error:", line?.ILCode, " ", line?.ILError) : string.Empty;
                casResponse += (header?.IHCode != "0000") ? string.Concat(header?.IHCode, " ", header?.IHError) : string.Empty;
                //Check if payment faced error in processing.

                //if (header?.IHCode != "0000")
                //{
                //    var subject = pay.ofm_name;
                //    //create Integration log with an error message.
                //  //  createIntregrationLogTasks.Add(CreateIntegrationErrorLog(subject, pay.ofm_application.ofm_applicationid.Value, casResponse, "CFS Integration Error", appUserService, d365WebApiService));
                //}

                //Update it with latest cas response.
                var invoiceToUpdate = new JsonObject {
                    {CcofInvoice.Fields.CcOf_Cas_Response, casResponse},
                    {CcofInvoice.Fields.StateCode,(int)((line?.ILCode=="0000" &&header?.IHCode=="0000") ?CcOf_Invoice_StateCode.Inactive:CcOf_Invoice_StateCode.Active)},
                    {CcofInvoice.Fields.StatusCode,(int)((line?.ILCode=="0000" && header?.IHCode=="0000")?CcOf_Invoice_StatusCode.Paid:CcOf_Invoice_StatusCode.ProcessingError)},
                    {CcofInvoice.Fields.CcOf_Revised_Invoice_Date,( header?.IHCode!="0000")?revisedInvoiceDate.ToString("yyyy-MM-dd"): null},
                    {CcofInvoice.Fields.CcOf_Revised_Invoice_Received_Date,( header?.IHCode!="0000")?revisedInvoiceReceivedDate.ToString("yyyy-MM-dd"):null },
                    {CcofInvoice.Fields.CcOf_Revised_Effective_Date,(header?.IHCode!="0000")?revisedEffectiveDate.ToString("yyyy-MM-dd"):null }
                };
                bool isPaid = line?.ILCode == "0000" && header?.IHCode == "0000";
                var relatedPaymentLines = serializedPayData?.Where(p =>p?.CcOf_Invoice != null && p.ccof_invoice == invoice?.ccof_invoiceid);




                if (relatedPaymentLines != null)
                {
                    foreach (var paymentLine in relatedPaymentLines)
                    {
                        
                        var paymentLineUpdate = new JsonObject { 
                        { D365PaymentLine.Fields.OfM_Cas_Response, casResponse },
                        { D365PaymentLine.Fields.StateCode, (int)((line?.ILCode == "0000" && header?.IHCode == "0000") ? OfM_Payment_StateCode.Inactive : OfM_Payment_StateCode.Active) },
                        { D365PaymentLine.Fields.StatusCode, (int)((line?.ILCode == "0000" && header?.IHCode == "0000") ? OfM_Payment_StatusCode.Paid : OfM_Payment_StatusCode.ProcessingError) },
                        { D365PaymentLine.Fields.OfM_Revised_Invoice_Date, header?.IHCode != "0000" ? revisedInvoiceDate.ToString("yyyy-MM-dd") : null },
                        { D365PaymentLine.Fields.OfM_Revised_Invoice_Received_Date, header?.IHCode != "0000" ? revisedInvoiceReceivedDate.ToString("yyyy-MM-dd") : null }, 
                        { D365PaymentLine.Fields.OfM_Revised_Effective_Date, header?.IHCode != "0000" ? revisedEffectiveDate.ToString("yyyy-MM-dd") : null } };

                        updatePaymentLineRequests.Add(new D365UpdateRequest(new D365EntityReference(D365PaymentLine.EntityLogicalCollectionName, paymentLine.ofm_paymentid),paymentLineUpdate ));
                    }
                }


                updateInvoiceRequests.Add(new D365UpdateRequest(new D365EntityReference(CcofInvoice.EntityLogicalCollectionName, invoice.ccof_invoiceid), invoiceToUpdate));
                
            }
        });

        var step2BatchResult = await d365WebApiService.SendBatchMessageAsync(appUserService.AZSystemAppUser, updateInvoiceRequests, null);
        var step3BatchResult = await d365WebApiService.SendBatchMessageAsync(appUserService.AZSystemAppUser, updatePaymentLineRequests, null);
        if (step2BatchResult.Errors.Any())
        {
            var errors = ProcessResult.Failure(ProcessId, step2BatchResult.Errors, step2BatchResult.TotalProcessed, step2BatchResult.TotalRecords);
            _logger.LogError(CustomLogEvent.Process, "Failed to update Invoices with an error: {error}", JsonValue.Create(errors)!.ToString());

            return errors.SimpleProcessResult;
        }
        if (step3BatchResult.Errors.Any())
        {
            var errors = ProcessResult.Failure(ProcessId, step2BatchResult.Errors, step2BatchResult.TotalProcessed, step2BatchResult.TotalRecords);
            _logger.LogError(CustomLogEvent.Process, "Failed to update Payments with an error: {error}", JsonValue.Create(errors)!.ToString());

            return errors.SimpleProcessResult;
        }
        //  await Task.WhenAll(createIntregrationLogTasks);
        return ProcessResult.Completed(ProcessId).SimpleProcessResult;
    }
}














