
using CCOF.Core.DataContext;
using CCOF.Infrastructure.WebAPI.Extensions;
using CCOF.Infrastructure.WebAPI.Messages;
using CCOF.Infrastructure.WebAPI.Models;
using CCOF.Infrastructure.WebAPI.Services.AppUsers;
using CCOF.Infrastructure.WebAPI.Services.D365WebApi;
using CCOF.Infrastructure.WebAPI.Services.D365WebAPI;
using CCOF.Infrastructure.WebAPI.Services.Processes;
using HandlebarsDotNet;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Extensions;
using System;
using System.Drawing.Text;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
//using ECC.Core.DataContext;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using static CCOF.Infrastructure.WebAPI.Extensions.Setup.Process;

namespace CCOF.Infrastructure.WebAPI.Services.Processes.Payments;

public class P500SendPaymentRequestProvider(IOptionsSnapshot<ExternalServices> bccasApiSettings, ID365AppUserService appUserService, ID365WebApiService d365WebApiService, ID365DataService dataService, ILoggerFactory loggerFactory, TimeProvider timeProvider) : ID365ProcessProvider
{
    private readonly BCCASApi _BCCASApi = bccasApiSettings.Value.BCCASApi;
    private readonly ID365AppUserService _appUserService = appUserService;
    private readonly ID365WebApiService _d365webapiservice = d365WebApiService;
    private readonly ILoggerFactory loggerFactory = loggerFactory;
    private readonly TimeProvider timeProvider = timeProvider;
    private readonly ILogger _logger = loggerFactory.CreateLogger(LogCategory.Process);
    private readonly ID365DataService _dataService = dataService;
  //  private readonly IPaymentValidator _paymentvalidator = paymentvalidator;

    private int _controlCount;
    private double _controlAmount;
    private int _oracleBatchNumber;
    private string _cgiBatchNumber = string.Empty;
   // private List<PaymentLine> erroredline= new List<PaymentLine>();
    private ProcessData? _data;
    private ProcessParameter? _processParams;
    private string _currentFiscalYearId;


    public Int16 ProcessId => Setup.Process.Payments.SendPaymentRequestId;
    public string ProcessName => Setup.Process.Payments.SendPaymentRequestName;

    public string RequestUri
    {
        get
        {
            // For reference only
            var fetchXml = $"""
                    <fetch version="1.0" output-format="xml-platform" mapping="logical" distinct="false" count="1">
                      <entity name="ofm_payment_file_exchange">
                        <attribute name="ccof_last_ccof_cgi_oracle_number" />
                        <attribute name="ofm_payment_file_exchangeid" />
                        <attribute name="owningbusinessunit" />
                        <attribute name="ofm_batch_number" />
                        <attribute name="ofm_oracle_batch_name" />
                        <attribute name="ofm_input_file_name" />
                        <order attribute="ofm_batch_number" descending="true" /> 
                      </entity>
                    </fetch>
                    """;

           
            var requestUri = $"""
                         ofm_payment_file_exchanges?fetchXml={WebUtility.UrlEncode(fetchXml)}
                         """;

            return requestUri;
        }
    }
    public string RequestCCOFPaymentLineUri
    {
        get
        {
            var fetchXml = $"""
                    <fetch>
                      <entity name="ofm_payment">
                        <attribute name="statecode" />
                        <attribute name="statuscode" />
                        <attribute name="ofm_paymentid" />
                        <filter>
                          <condition attribute="statecode" operator="eq" value="0" />
                        </filter>
                        <link-entity name="ccof_invoice" from="ccof_invoiceid" to="ccof_invoice" link-type="inner" alias="invoice">
                          <attribute name="ccof_coding_line_type" />
                          <attribute name="ccof_invoiceid" />
                          <attribute name="ccof_organization" />
                          <attribute name="ccof_payment_type" />
                          <filter>
                            <condition attribute="statecode" operator="eq" value="0" />
                            <condition attribute="statuscode" operator="eq" value="{(int)CcOf_Invoice_StatusCode.Approved}" />
                            <condition attribute="owningbusinessunitname" operator="like" value="%CCOF%" />
                          </filter>
                        </link-entity>
                      </entity>
                    </fetch>
                    """;
            var requestUri = $"""
                         ofm_payments?fetchXml={WebUtility.UrlEncode(fetchXml)}
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
                          <condition attribute="statuscode" operator="eq" value="{(int)CcOf_Invoice_StatusCode.Approved}" />
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
   
    public string RequestACKCodeUri
    {
        get
        {
            // For reference only
            var fetchXml = $"""
                    <fetch>
                      <entity name="ofm_ack_codes">
                        <attribute name="ofm_ack_number" />
                        <attribute name="ofm_cohortid" />
                        <attribute name="ofm_payment_type" />
                      </entity>
                    </fetch>
                    """;

            var requestUri = $"""
                         ofm_ack_codeses?$select=ofm_ack_number,_ofm_cohortid_value,ofm_payment_type
                         """;

            return requestUri;
        }
    }

    public async Task<ProcessData> GetDataAsync()
    {
        _logger.LogDebug(CustomLogEvent.Process, "Calling GetData of {nameof}", nameof(P500SendPaymentRequestProvider));

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
            if (currentValue?.AsArray().Count == 0)
            {
                _logger.LogInformation(CustomLogEvent.Process, "No records found");
            }
            d365Result = currentValue!;
        }

        _data = new ProcessData(d365Result);

        _logger.LogDebug(CustomLogEvent.Process, "Query Result {_data}", _data.Data.ToJsonString());

        return await Task.FromResult(_data);
    }

   
    public async Task<ProcessData> GetPaymentLineData()
    {
        _logger.LogDebug(CustomLogEvent.Process, "Calling GetData of {nameof}", nameof(P500SendPaymentRequestProvider));


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
        }

        _data = new ProcessData(d365Result);

        _logger.LogDebug(CustomLogEvent.Process, "Query Result {_data}", _data.Data.ToJsonString());

        return await Task.FromResult(_data);
    }
    public async Task<ProcessData> GetCCOFPaymentLineData()
    {
        _logger.LogDebug(CustomLogEvent.Process, "Calling GetData of {nameof}", nameof(P500SendPaymentRequestProvider));


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
        }

        _data = new ProcessData(d365Result);

        _logger.LogDebug(CustomLogEvent.Process, "Query Result {_data}", _data.Data.ToJsonString());

        return await Task.FromResult(_data);
    }
    public async Task<JsonObject> RunProcessAsync(ID365AppUserService appUserService, ID365WebApiService d365WebApiService, ProcessParameter processParams)
    {
        _processParams = processParams;
        List<InvoiceHeader> invoiceHeaders = [];
        List<List<InvoiceHeader>> headerList = [];
        List<CcofInvoice> serializedInvoiceData = [];
        List<D365PaymentLine> CCOFPaymentLines = [];  // for CCOF Paymentlines
        List<InvoiceCommentLines> invoiceCommentLines = [];
        CcofInvoice eachline ;
         var line = typeof(InvoiceLines);
        var commentline = typeof(InvoiceCommentLines);
        var header = typeof(InvoiceHeader);
        string inboxFileBytes = string.Empty;

        #region Step 0.1: Get paymentlines data & current Financial Year
        try
        {
            var invoiceData = await GetPaymentLineData();
            serializedInvoiceData = JsonSerializer.Deserialize<List<CcofInvoice>>(invoiceData.Data.ToString());
            var grouppayment = serializedInvoiceData?.GroupBy(p => p.ccof_invoice_number).ToList();
            //var fiscalyear = serializedPaymentData?.FirstOrDefault()?.ofm_fiscal_year.ofm_financial_year;
            var fiscalyear = "2026";
            var ccofPaymentLineData = await GetCCOFPaymentLineData();
            CCOFPaymentLines = JsonSerializer.Deserialize<List<D365PaymentLine>>(ccofPaymentLineData.Data.ToString());
            #endregion

            #region Step 0.2: Get latest Oracle Batch Number



            string oracleBatchName;
            var latestPaymentFileExchangeData = await GetDataAsync();
            var serializedPFXData = JsonSerializer.Deserialize<List<ofm_payment_file_exchange>>(latestPaymentFileExchangeData.Data.ToString());

            if (serializedPFXData is not null && serializedPFXData.Count != 0 && serializedPFXData[0].ccof_last_cgi_oracle_number != null)
            {
                _oracleBatchNumber = Convert.ToInt32(serializedPFXData[0].ccof_last_cgi_oracle_number) + 1;
                oracleBatchName = _BCCASApi.clientCode  + "CCOF" + (_oracleBatchNumber).ToString("D5");

                _cgiBatchNumber = (Convert.ToInt32(serializedPFXData[0].OfmBatchNumber)).ToString("D9").Substring(0, 9);
            }
            else
            {
                _oracleBatchNumber = Convert.ToInt32(_BCCASApi.oracleBatchNumber);
                oracleBatchName = _BCCASApi.clientCode + "CCOF" + _BCCASApi.oracleBatchNumber;
                _cgiBatchNumber = (Convert.ToInt32(serializedPFXData[0].OfmBatchNumber)).ToString("D9").Substring(0, 9);
                
            }

            #endregion

            #region Step 0.3: Get ACK Codes

            //IEnumerable<Ack_Codes> ackCode = await LoadACKCodeAsync();
            var ackCode = await GetACKCodes();

            #endregion

            #region Step 1: Handlebars format to generate Inbox data

            string source = @"
{{feederNumber}}{{batchType}}{{transactionType}}{{delimiter}}{{feederNumber}}{{fiscalYear}}{{cGIBatchNumber}}{{messageVersionNumber}}{{delimiter}}
{{#each InvoiceHeader}}{{this.feederNumber}}{{this.batchType}}{{this.headertransactionType}}{{this.delimiter}}{{this.supplierNumber}}{{this.supplierSiteNumber}}{{this.invoiceNumber}}{{this.PONumber}}{{this.invoiceType}}{{this.invoiceDate}}{{this.payGroupLookup}}{{this.remittanceCode}}{{this.grossInvoiceAmount}}{{this.CAD}}{{this.invoiceDate}}{{this.termsName}}{{this.description}}{{this.goodsDate}}{{this.invoiceRecDate}}{{this.oracleBatchName}}{{this.SIN}}{{this.payflag}}{{this.flow}}{{this.delimiter}}
{{#each InvoiceLines}}{{this.feederNumber}}{{this.batchType}}{{this.linetransactionType}}{{this.delimiter}}{{this.supplierNumber}}{{this.supplierSiteNumber}}{{this.invoiceNumber}}{{this.invoiceLineNumber}}{{this.committmentLine}}{{this.lineAmount}}{{this.lineCode}}{{this.distributionACK}}{{this.lineDescription}}{{this.effectiveDate}}{{this.quantity}}{{this.unitPrice}}{{this.optionalData}}{{this.distributionSupplierNumber}}{{this.flow}}{{this.delimiter}}
{{/each}}{{#each InvoiceCommentLines}}{{this.feederNumber}}{{this.batchType}}{{this.linetransactionType}}{{this.delimiter}}{{this.supplierNumber}}{{this.supplierSiteNumber}}{{this.invoiceNumber}}{{this.CommentLineNumber}}{{this.Comment}}{{this.delimiter}}
{{/each}}{{/each}}{{feederNumber}}{{batchType}}{{trailertransactionType}}{{delimiter}}{{feederNumber}}{{fiscalYear}}{{cGIBatchNumber}}{{controlCount}}{{controlAmount}}{{delimiter}}
";


            var template = Handlebars.Compile(source);

            // add invoice header for each organization and invoice lines for each facility
            foreach (var headeritem in grouppayment)
            {

                var pay_method = (ECc_Payment_Method)headeritem.First().ccof_paymentmethod;

                var paymentType = ((ECc_Payment_Type)headeritem.First().ccof_payment_type);
                // from payment line

                string ackNumber = string.Empty;
                var ackJsonArray = ackCode.Data.AsArray(); // convert JsonNode to JsonArray

                var ackCodeList = JsonSerializer.Deserialize<List<Ack_Codes>>(
                    ackJsonArray.ToJsonString(),
                    Setup.s_writeOptionsForLogs
                )!;
                var filteredAck = ackCodeList
    .Where(ack => ack.OfmPaymentType == (int)paymentType)
    .ToList();
                

                //  var ackCodeList = ackCode?.Where(ack => ack.ofm_payment_type == paymentType.Get).ToList();

                if (filteredAck.Any() && filteredAck.Count > 1)
                {
                    ackNumber = filteredAck.Select(code => code.OfmAckNumber).FirstOrDefault();
                }
              //  ackNumber = "0622265006500650822007400000000000";
                double invoiceamount = 0.00;
                List<InvoiceLines> invoiceLines = [];
                


                foreach (var lineitem in headeritem.Select((item, i) => (item, i)))
                {
                    eachline = lineitem.item;
                    try
                    {
                   
                        string remittanceMessage = Regex.Replace(lineitem.item.ccof_remittancemessage ?? string.Empty, @"\s+",string.Empty);

                        List<InvoiceCommentLines> headerCommentLines = new();

                        int maxLineLength = 40;
                        int maxLines = 10;

                        int lineCount = 0;
                        int CommentLineNumber = 1;
                        invoiceamount = invoiceamount + Convert.ToDouble(lineitem.item.ccof_grand_total);//line amount should come from invoice
                        var paytype = lineitem.item.ccof_payment_typename;
                      
                        invoiceLines.Add(new InvoiceLines
                        {
                            feederNumber = _BCCASApi.feederNumber,// Static value:3540
                            batchType = _BCCASApi.batchType,//Static  value :AP
                            delimiter = _BCCASApi.delimiter,//Static value:\u001d
                            linetransactionType = _BCCASApi.InvoiceLines.linetransactionType,//Static value:IL for each line
                            invoiceNumber = lineitem.item.ccof_invoice_number.PadRight(line.FieldLength("invoiceNumber")),// Autogenerated and unique for supplier transaction
                            invoiceLineNumber = (lineitem.i + 1).ToString("D4"),// Incremented by 1 for each line in case for multiple lines
                            supplierNumber = lineitem.item.ccof_supplier_number.PadRight(line.FieldLength("supplierNumber")),// Populate from Organization Supplier info
                            supplierSiteNumber = lineitem.item.ccof_site_number.PadLeft(line.FieldLength("supplierSiteNumber"), '0'),// Populate from Organization Supplier info
                            committmentLine = _BCCASApi.InvoiceLines.committmentLine,//Static value:0000
                            lineAmount = (lineitem.item.ccof_grand_total.Value < 0 ? "-" : "") + Math.Abs(lineitem.item.ccof_grand_total.Value).ToString("0.00", System.Globalization.CultureInfo.InvariantCulture).PadLeft(line.FieldLength("lineAmount") - (lineitem.item.ccof_grand_total.Value < 0 ? 1 : 0), '0'),// come from split funding amount per facility
                            lineCode = (lineitem.item.ccof_grand_total.Value > 0 ? "D" : "C"),//if it is positive then line code is Debit otherwise credit
                                                                                  //distributionACK = _BCCASApi.InvoiceLines.distributionACK.PadRight(line.FieldLength("distributionACK")),// using test data shared by CAS,should be changed for prod
                            distributionACK = ackNumber.PadRight(line.FieldLength("distributionACK")), //fetching from ACK Codes from dataverse based on payment type and cohort
                            lineDescription = (lineitem.item.ccof_payment_type).ToString().PadRight(line.FieldLength("lineDescription")), // Pouplate extra info from facility
                            effectiveDate = lineitem.item.ccof_revised_effective_date?.ToString("yyyyMMdd") ?? lineitem.item.ccof_effective_date?.ToString("yyyyMMdd"),//same as invoice date
                            quantity = _BCCASApi.InvoiceLines.quantity,//Static Value:0000000.00 not used by feeder
                            unitPrice = _BCCASApi.InvoiceLines.unitPrice,//Static Value:000000000000.00 not used by feeder
                            optionalData = string.Empty.PadRight(line.FieldLength("optionalData")),// PO ship to asset tracking values are set to blank as it is optional
                            distributionSupplierNumber = lineitem.item.ccof_supplier_number.PadRight(line.FieldLength("distributionSupplierNumber")),// Supplier number from Organization
                            flow = string.Empty.PadRight(line.FieldLength("flow")), //can be use to pass additional info from facility or application
                        });
                        _controlCount++;
                        for (int i = 0; i < remittanceMessage.Length && lineCount < maxLines; i += maxLineLength)
                        {
                            string messagePart = remittanceMessage.Substring(
                                i,
                                Math.Min(maxLineLength, remittanceMessage.Length - i)
                            );

                            headerCommentLines.Add(new InvoiceCommentLines
                            {
                                feederNumber = _BCCASApi.feederNumber,
                                batchType = _BCCASApi.batchType,
                                delimiter = _BCCASApi.delimiter,
                                linetransactionType = _BCCASApi.InvoiceCommentLines.linetransactionType,
                                invoiceNumber = lineitem.item.ccof_invoice_number
                                    .PadRight(line.FieldLength("invoiceNumber")),
                                supplierNumber = lineitem.item.ccof_supplier_number
                                    .PadRight(line.FieldLength("supplierNumber")),
                                supplierSiteNumber = lineitem.item.ccof_site_number
                                    .PadLeft(line.FieldLength("supplierSiteNumber"), '0'),
                                CommentLineNumber = CommentLineNumber.ToString("D4"),
                                Comment = messagePart.PadRight(line.FieldLength("Comment"))
                            });

                            _controlCount++;
                            lineCount++;
                            CommentLineNumber++;
                        }

                        invoiceHeaders.Add(new InvoiceHeader
                        {
                            feederNumber = _BCCASApi.feederNumber,// Static value:3540
                            batchType = _BCCASApi.batchType,//Static  value :AP
                            headertransactionType = _BCCASApi.InvoiceHeader.headertransactionType,//Static value:IH for each header
                            delimiter = _BCCASApi.delimiter,//Static value:\u001d
                            supplierNumber = headeritem.First().ccof_supplier_number.PadRight(header.FieldLength("supplierNumber")),// Populate from Organization Supplier info
                            supplierSiteNumber = headeritem.First().ccof_site_number.PadLeft(header.FieldLength("supplierSiteNumber"), '0'),// Populate from Organization Supplier info
                            invoiceNumber = headeritem.First().ccof_invoice_number.PadRight(header.FieldLength("invoiceNumber")),// Autogenerated and unique for supplier transaction
                            PONumber = string.Empty.PadRight(header.FieldLength("PONumber")),// sending blank as not used by feeder
                            invoiceDate = headeritem.First().ccof_revised_invoice_date?.ToString("yyyyMMdd") ?? headeritem.First().ccof_invoice_date?.ToString("yyyyMMdd"), // set to current date
                            invoiceType = invoiceamount < 0 ? "CM" : "ST",// static to ST (standard invoice)
                            payGroupLookup = string.Concat("GEN ", pay_method, " N"),//GEN CHQ N if using cheque or GEN EFT N if direct deposit
                            remittanceCode = _BCCASApi.InvoiceHeader.remittanceCode.PadRight(header.FieldLength("remittanceCode")), // for payment stub it is 00 always.
                            grossInvoiceAmount = (invoiceamount < 0 ? "-" : "") + Math.Abs(invoiceamount).ToString("0.00", System.Globalization.CultureInfo.InvariantCulture).PadLeft(header.FieldLength("grossInvoiceAmount") - (invoiceamount < 0 ? 1 : 0), '0'), // invoice amount come from OFM total base value.
                            CAD = _BCCASApi.InvoiceHeader.CAD,// static value :CAD
                            termsName = _BCCASApi.InvoiceHeader.termsName.PadRight(header.FieldLength("termsName")),//setting it to immediate for successful testing, this needs to be dynamic going forward.
                            goodsDate = string.Empty.PadRight(header.FieldLength("goodsDate")),//optional field so set to null
                            invoiceRecDate = headeritem.First().ccof_revised_invoice_received_date?.ToString("yyyyMMdd") ?? headeritem.First().ccof_invoice_received_date?.ToString("yyyyMMdd"),// 5 days from invoice date
                            oracleBatchName = (_BCCASApi.clientCode + "CCOF" + (_oracleBatchNumber).ToString("D5")).PadRight(header.FieldLength("oracleBatchName")),//6225OFM00001 incremented by 1 for each header
                            SIN = string.Empty.PadRight(header.FieldLength("SIN")), //optional field set to blank
                            payflag = _BCCASApi.InvoiceHeader.payflag,// Static value: Y (separate chq for each line)
                            description = Regex.Replace(headeritem.First()?.ccof_organization_id, @"[^\w $\-]", "").PadRight(header.FieldLength("description")).Substring(0, header.FieldLength("description")),// can be used to pass extra info
                            flow = string.Empty.PadRight(header.FieldLength("flow")),// can be used to pass extra info
                            invoiceLines = invoiceLines,
                            InvoiceCommentLines = headerCommentLines
                           
                        });
                        _controlAmount = _controlAmount + invoiceamount;
                        _controlCount++;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);

                    }
                }

            }

            // break transaction list into multiple list if it contains more than 250 transactions
            headerList = invoiceHeaders
            .Select((x, i) => new { Index = i, Value = x })
            .GroupBy(x => x.Index / _BCCASApi.transactionCount)
            .Select(x => x.Select(v => v.Value).ToList())
            .ToList();

            #endregion

            #region Step 2: Compose the inbox file string
            List<CcofInvoice> paylinesToUpdate = new List<CcofInvoice>();
            // for each set of transaction create and upload inbox file in payment file exchange
            foreach (List<InvoiceHeader> headeritem in headerList)
            {
                _cgiBatchNumber = ((Convert.ToInt32(_cgiBatchNumber)) + 1).ToString("D9"); // Increase the CGI Batch Number by 1 

                headeritem.ForEach(x =>
                {

                    foreach (var paydata in serializedInvoiceData.Where(paydata => paydata.ccof_invoice_number == x.invoiceLines.First().invoiceNumber.TrimEnd()))
                    {
                        paydata.ccof_batch_number = _cgiBatchNumber;
                        paylinesToUpdate.Add(paydata);
                    }
                });

                _controlAmount = (double)headeritem.SelectMany(x => x.invoiceLines).Sum(x => Convert.ToDecimal(x.lineAmount));
                _controlCount =
                    headeritem.Count                                         // IH
                  + headeritem.SelectMany(x => x.invoiceLines).Count()       // IL
                  + headeritem.SelectMany(x => x.InvoiceCommentLines).Count();                              // IC 

                var data = new
                {
                    feederNumber = _BCCASApi.feederNumber,// Static value:3540
                    batchType = _BCCASApi.batchType,//Static  value :AP
                    delimiter = _BCCASApi.delimiter,//Static value:\u001d
                    transactionType = _BCCASApi.transactionType,//Static  value :BH
                    fiscalYear = fiscalyear,//current fiscal year
                    cGIBatchNumber = _cgiBatchNumber,//unique autogenerated number
                    messageVersionNumber = _BCCASApi.messageVersionNumber,//Static  value :0001
                    controlCount = _controlCount.ToString("D15"),// total number of lines count except BH and BT
                    controlAmount = _controlAmount.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture).PadLeft(header.FieldLength("grossInvoiceAmount"), '0'),// total sum of amount
                    trailertransactionType = _BCCASApi.trailertransactionType,//Static  value :BT
                    InvoiceHeader = headeritem
                    
                };

                inboxFileBytes += template(data);
            }

            #endregion

            #region  Step 3: Create Payment File Exchange Record for all approved payment lines and Mark Processed Payment Lines

            if (!string.IsNullOrEmpty(inboxFileBytes))
            {
                bool savePFEResult = await SaveInboxFileOnNewPaymentFileExchangeRecord(appUserService, d365WebApiService, _BCCASApi.feederNumber, inboxFileBytes);

                if (savePFEResult)
                    await MarkPaymentLinesAsProcessed(appUserService, d365WebApiService, paylinesToUpdate);
                await MarkCCOFPaymentLinesAsProcessed(appUserService, d365WebApiService, CCOFPaymentLines);
            }

            #endregion
        }
        catch (Exception ex)
        {
            //var opsuserID = await _paymentvalidator.GetOpssupervisorEmail(true);
            //var ccuserID = await _paymentvalidator.GetccuserEmail(true);

            //await _paymentvalidator.SendPaymentErrorEmail(500, "500",(Guid)(_processParams.Notification.SenderId));
            Console.WriteLine(ex.Message);

        }
        return ProcessResult.Completed(ProcessId).SimpleProcessResult;
    }

    private async Task<JsonObject> MarkPaymentLinesAsProcessed(ID365AppUserService appUserService, ID365WebApiService d365WebApiService, List<CcofInvoice> payments)
    {
        var updatePayRequests = new List<HttpRequestMessage>() { };
        payments.ForEach(pay =>
        {
            var payToUpdate = new JsonObject {
                  { "statuscode", Convert.ToInt32(CcOf_Invoice_StatusCode.ProcessingPayment) },

                  {"ccof_batch_number",pay.ccof_batch_number }
             };

            updatePayRequests.Add(new D365UpdateRequest(new D365EntityReference(CcOf_Invoice.EntityLogicalCollectionName, pay.ccof_invoiceid), payToUpdate));
        });

        var paymentBatchResult = await d365WebApiService.SendBatchMessageAsync(appUserService.AZSystemAppUser, updatePayRequests, null);
        if (paymentBatchResult.Errors.Any())
        {
            var errors = ProcessResult.Failure(ProcessId, paymentBatchResult.Errors, paymentBatchResult.TotalProcessed, paymentBatchResult.TotalRecords);
            _logger.LogError(CustomLogEvent.Process, "Failed to update invoice status with an error: {error}", JsonValue.Create(errors)!.ToString());

            return errors.SimpleProcessResult;
        }

        return paymentBatchResult.SimpleBatchResult;
    }
    private async Task<JsonObject> MarkCCOFPaymentLinesAsProcessed(ID365AppUserService appUserService, ID365WebApiService d365WebApiService, List<D365PaymentLine> payments)
    {
        var updatePayRequests = new List<HttpRequestMessage>() { };
        payments.ForEach(pay =>
        {
            var payToUpdate = new JsonObject {
                  { "statuscode", Convert.ToInt32(OfM_Payment_StatusCode.ProcessingPayment) }
             };
            updatePayRequests.Add(new D365UpdateRequest(new D365EntityReference(D365PaymentLine.EntityLogicalCollectionName, pay.ofm_paymentid), payToUpdate));
        });

        var paymentBatchResult = await d365WebApiService.SendBatchMessageAsync(appUserService.AZSystemAppUser, updatePayRequests, null);
        if (paymentBatchResult.Errors.Any())
        {
            var errors = ProcessResult.Failure(ProcessId, paymentBatchResult.Errors, paymentBatchResult.TotalProcessed, paymentBatchResult.TotalRecords);
            _logger.LogError(CustomLogEvent.Process, "Failed to update invoice status with an error: {error}", JsonValue.Create(errors)!.ToString());

            return errors.SimpleProcessResult;
        }

        return paymentBatchResult.SimpleBatchResult;
    }
    private async Task<bool> SaveInboxFileOnNewPaymentFileExchangeRecord(ID365AppUserService appUserService, ID365WebApiService d365WebApiService, string feederNumber, string result)
    {
        var inboxFileName = ("INBOX.F" + feederNumber + "." + DateTime.UtcNow.ToLocalPST().ToString("yyyyMMddHHmmss"));
        var requestBody = new JsonObject()
        {
            ["ofm_input_file_name"] = inboxFileName,
            ["ofm_name"] = inboxFileName,
            ["ofm_batch_number"] = _cgiBatchNumber,
            ["ccof_last_ccof_cgi_oracle_number"] = _oracleBatchNumber.ToString()
            //["ofm_fiscal_year@odata.bind"] = $"/ofm_fiscal_years(abf35f80-5499-ee11-be37-000d3a09d499)"
        };

        var pfeCreateResponse = await d365WebApiService.SendCreateRequestAsync(appUserService.AZSystemAppUser, OfM_Payment_File_Exchange.EntitySetName, requestBody.ToString());

        if (!pfeCreateResponse.IsSuccessStatusCode)
        {
            var pfeCreateError = await pfeCreateResponse.Content.ReadAsStringAsync();
            _logger.LogError(CustomLogEvent.Process, "Failed to create payment file exchange record with the server error {responseBody}", JsonValue.Create(pfeCreateError)!.ToString());

            return await Task.FromResult(false);
        }

        var pfeRecord = await pfeCreateResponse.Content.ReadFromJsonAsync<JsonObject>();

        if (pfeRecord is not null && pfeRecord.ContainsKey(OfM_Payment_File_Exchange.Fields.OfM_Payment_File_ExchangeId))
        {
            if (inboxFileName.Length > 0)
            {
                // Update the new Payment File Exchange record with the new document
                HttpResponseMessage pfeUpdateResponse = await _d365webapiservice.SendDocumentRequestAsync(_appUserService.AZPortalAppUser, OfM_Payment_File_Exchange.EntitySetName,
                                                                                                    new Guid(pfeRecord[OfM_Payment_File_Exchange.Fields.OfM_Payment_File_ExchangeId].ToString()),
                                                                                                    Encoding.ASCII.GetBytes(result.TrimEnd()),
                                                                                                    inboxFileName);

                if (!pfeUpdateResponse.IsSuccessStatusCode)
                {
                    var pfeUpdateError = await pfeUpdateResponse.Content.ReadAsStringAsync();
                    _logger.LogError(CustomLogEvent.Process, "Failed to update payment file exchange record with the new inbox document with the server error {responseBody}", pfeUpdateError.CleanLog());

                    return await Task.FromResult(false);
                }
            }
        }
        return await Task.FromResult(true);
    }
    //private async Task<IEnumerable<Ack_Codes>> LoadACKCodeAsync()
    //{
    //    var localdata = await _dataService.FetchDataAsync(RequestACKCodeUri, "ACK_Codes");
    //    // var deserializedData = localdata.Data.Deserialize<List<Ack_Codes>>(Setup.s_writeOptionsForLogs);
    //    var deserializedData = JsonSerializer.Deserialize<List<Ack_Codes>>(localdata.Data.ToString());
    //    return await Task.FromResult(deserializedData!);
    //}

    public async Task<ProcessData> GetACKCodes()
    {
        _logger.LogDebug(CustomLogEvent.Process, "Calling GetData of {nameof}", nameof(P500SendPaymentRequestProvider));


        var response = await _d365webapiservice.SendRetrieveRequestAsync(_appUserService.AZSystemAppUser, RequestACKCodeUri, isProcess: true);
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
        }

        _data = new ProcessData(d365Result);

        _logger.LogDebug(CustomLogEvent.Process, "Query Result {_data}", _data.Data.ToJsonString());

        return await Task.FromResult(_data);
    }
}