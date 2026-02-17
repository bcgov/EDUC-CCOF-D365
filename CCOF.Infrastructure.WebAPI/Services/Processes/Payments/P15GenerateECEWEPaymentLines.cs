using CCOF.Infrastructure.WebAPI.Extensions;
using CCOF.Infrastructure.WebAPI.Models;

using CCOF.Infrastructure.WebAPI.Services.AppUsers;
using CCOF.Infrastructure.WebAPI.Services.D365WebApi;
using System.Net;
using System.Text.Json.Nodes;
using System.Text.Json;

using Microsoft.Extensions.Options;


namespace CCOF.Infrastructure.WebAPI.Services.Processes.Payments
{
    public class P515GenerateECEWEPaymentLinesProvider(IOptionsSnapshot<ExternalServices> bccasApiSettings, ID365AppUserService appUserService, ID365WebApiService d365WebApiService, ILoggerFactory loggerFactory) : ID365ProcessProvider
    {
        private readonly ID365AppUserService _appUserService = appUserService;
        private readonly ID365WebApiService _d365WebApiService = d365WebApiService;
        private readonly ILogger _logger = loggerFactory.CreateLogger(LogCategory.Process);
        private ProcessParameter? _processParams;
        public Int16 ProcessId => Setup.Process.Payments.GenerateECEWEPaymentLinesId;
        public string ProcessName => Setup.Process.Payments.GenerateECEWEPaymentLinesName;
        public string CodingLineType_FetchParameter;
        #region Data Queries
        public string MonthlyECEWEReportUri
        {
            get
            {
                var fetchXml = $$"""
                    <fetch version="1.0" output-format="xml-platform" mapping="logical" distinct="false">
                      <entity name="ccof_ece_monthly_report">
                        <attribute name="statecode"/>
                        <attribute name="ccof_ece_monthly_reportid"/>
                        <attribute name="ccof_name"/>
                        <attribute name="createdon"/>
                        <attribute name="ccof_total_amount"/>
                        <attribute name="ccof_we_subtotal"/>
                        <attribute name="ccof_sb_subtotal"/>
                        <attribute name="ccof_approval_date"/>
                        <attribute name="ccof_verification_date"/>
                        <attribute name="ccof_facility"/>
                        <attribute name="ccof_fiscal_year"/>
                        <attribute name="ccof_organization"/>
                        <attribute name="ccof_month"/>
                        <attribute name="ccof_report_type"/>
                        <attribute name="ccof_year"/>
                        <attribute name="statuscode"/>
                        <order attribute="ccof_name" descending="false"/>    <filter type="and">
                          <condition attribute="statecode" operator="eq" value="0"/>
                          <condition attribute="ccof_ece_monthly_reportid" operator="eq" value="{{_processParams.MonthlyECEWEReportId.ToString()}}"/>
                        </filter>    <link-entity 
                            name="ccof_program_year" 
                            alias="programYear" 
                            link-type="inner" 
                            from="ccof_program_yearid" 
                            to="ccof_fiscal_year">
                          <attribute name="ccof_name" />
                          <attribute name="ccof_programyearnumber" />
                          <attribute name="statuscode" />
                        </link-entity>
                    <link-entity name="account" from="accountid" to="ccof_facility" link-type="inner" alias="facility">
                      <attribute name="name" />
                      <attribute name="accountnumber" />
                    </link-entity>
                      </entity>
                    </fetch>
                    """;
                var requestUri = $"""
                         ccof_ece_monthly_reports?fetchXml={WebUtility.UrlEncode(fetchXml)}
                         """;
                return requestUri;
            }
        }
        public string CodingLineTypeRequestUri
        {
            get
            {
                var fetchXml = $$"""
                    <fetch version="1.0" output-format="xml-platform" mapping="logical" distinct="false">
                      <entity name="ccof_coding_line_type">
                        <attribute name="ccof_coding_line_typeid" />
                        <attribute name="ccof_name" />
                        <attribute name="ccof_coding_line_type" />
                        <attribute name="statuscode" />
                        <attribute name="owningbusinessunit" />
                        <attribute name="ownerid" />
                        <attribute name="statecode" />
                        <attribute name="createdon" />
                        <attribute name="createdby" />
                        <order attribute="createdon" descending="false" />
                        <filter type="and">
                          <condition attribute="statecode" operator="eq" value="0" />
                          <condition attribute="ccof_coding_line_type" operator="like" value="%{{CodingLineType_FetchParameter}}%" />
                        </filter>
                      </entity>
                    </fetch>
                    """;

                var requestUri = $"""
                         ccof_coding_line_types?fetchXml={WebUtility.UrlEncode(fetchXml)}
                         """;

                return requestUri;
            }
        }
        #endregion
        public async Task<ProcessData> GetCodingLineTypeDataAsync()
        {
            _logger.LogDebug(CustomLogEvent.Process, nameof(GetCodingLineTypeDataAsync));

            var response = await _d365WebApiService.SendRetrieveRequestAsync(_appUserService.AZSystemAppUser, CodingLineTypeRequestUri, false, 0, true);

            if (!response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                _logger.LogError(CustomLogEvent.Process, "Failed to query CodingLineType record information with the server error {responseBody}", responseBody.CleanLog());

                return await Task.FromResult(new ProcessData(string.Empty));
            }

            var jsonObject = await response.Content.ReadFromJsonAsync<JsonObject>();

            JsonNode d365Result = string.Empty;
            if (jsonObject?.TryGetPropertyValue("value", out var currentValue) == true)
            {
                if (currentValue?.AsArray().Count == 0)
                {
                    _logger.LogInformation(CustomLogEvent.Process, "No records found with query {requestUri}", CodingLineTypeRequestUri.CleanLog());
                }
                d365Result = currentValue!;
            }

            _logger.LogDebug(CustomLogEvent.Process, "Query Result {queryResult}", d365Result.ToString().CleanLog());

            return await Task.FromResult(new ProcessData(d365Result));
        }
        public async Task<ProcessData> GetDataAsync()
        {
            _logger.LogDebug(CustomLogEvent.Process, "GetDataAsync");

            var response = await _d365WebApiService.SendRetrieveRequestAsync(_appUserService.AZSystemAppUser, MonthlyECEWEReportUri);

            if (!response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                _logger.LogError(CustomLogEvent.Process, "Failed to query Expense record information with the server error {responseBody}", responseBody.CleanLog());

                return await Task.FromResult(new ProcessData(string.Empty));
            }

            var jsonObject = await response.Content.ReadFromJsonAsync<JsonObject>();

            JsonNode d365Result = string.Empty;
            if (jsonObject?.TryGetPropertyValue("value", out var currentValue) == true)
            {
                if (currentValue?.AsArray().Count == 0)
                {
                    _logger.LogInformation(CustomLogEvent.Process, "No records found with query {requestUri}", MonthlyECEWEReportUri.CleanLog());
                }
                d365Result = currentValue!;
            }

            _logger.LogDebug(CustomLogEvent.Process, "Query Result {queryResult}", d365Result.ToString().CleanLog());

            return await Task.FromResult(new ProcessData(d365Result));
        }
        private async Task<JsonObject> CreateSinglePayment(JsonNode monthlyECEWEReport, DateTime paymentDate, decimal? totalAmount, int invoiceLineNumber, int paymentType)
        {
            DateTime invoiceDate = paymentDate;
            DateTime invoiceReceivedDate = invoiceDate;
            DateTime effectiveDate = invoiceDate;
            string paymentTypeString = paymentType switch
            {
               
               9 => "ECEWE",
                11 => "ECESB",
                _ => "Unknown"
            };
            var multiKeyDict = new Dictionary<(string, int, string), string>()  
            {
                { ("ECESB", 1, "positive"), "CC"},
                { ("ECESB", 1, "negative"), "CD"},
                { ("ECESB", 4, "positive"), "PC"},
                { ("ECESB", 4, "negative"), "PD"},
                { ("ECEWE", 1, "positive"), "CW"},
                { ("ECEWE", 1, "negative"), "CZ"},
                { ("ECEWE", 4, "positive"), "PW"},
                { ("ECEWE", 4, "negative"), "PZ"},

            };
            int statuscodeFY = (int)monthlyECEWEReport["programYear.statuscode"];    // 1 - Current, 4 - Historical
            string totalAmountSign = totalAmount >= 0 ? "positive" : "negative";
            CodingLineType_FetchParameter = multiKeyDict[(paymentTypeString, statuscodeFY, totalAmountSign)];
            var codingLineTypeString = await GetCodingLineTypeDataAsync();
            var codingLineTypeData = JsonSerializer.Deserialize<JsonArray>(codingLineTypeString.Data.ToString());
            var codingLineTypeRecord = codingLineTypeData!.First();
            //var codingLineTypeLabel = codingLineTypeRecord!["ccof_coding_line_type"];
            var codingLineTypeId = codingLineTypeRecord!["ccof_coding_line_typeid"];
            var payload = new JsonObject()
                        {
                            { "ofm_invoice_line_number", invoiceLineNumber},
                            { "ofm_amount", totalAmount},
                            { "ofm_payment_type", paymentType},
                            { "ccof_monthly_ecewe_report@odata.bind",$"/ccof_ece_monthly_reports({monthlyECEWEReport["ccof_ece_monthly_reportid"]})" },
                            { "ofm_invoice_date", invoiceDate.ToString("yyyy-MM-dd") },
                            { "ofm_invoice_received_date", invoiceReceivedDate.ToString("yyyy-MM-dd")},
                            { "ofm_effective_date", effectiveDate.ToString("yyyy-MM-dd")},
                            { "ccof_program_year@odata.bind",$"/ccof_program_years({monthlyECEWEReport["_ccof_fiscal_year_value"]})" },
                            { "ccof_coding_line_type@odata.bind",$"/ccof_coding_line_types({codingLineTypeId})" },
                            { "statuscode",4  }, //Approved for Payment in PaymentLine table
                            { "ofm_facility@odata.bind", $"/accounts({monthlyECEWEReport["_ccof_facility_value"]})" },
                            { "ofm_organization@odata.bind", $"/accounts({monthlyECEWEReport["_ccof_organization_value"]})" },
                            { "ofm_description", $"{monthlyECEWEReport?["facility.name"] +" " +monthlyECEWEReport?["ccof_month"]+"/"+monthlyECEWEReport?["ccof_year"]+" " +paymentTypeString}" },
                        };
            var requestBody = JsonSerializer.Serialize(payload);
            _logger.LogInformation("Payment payload: {payload}", requestBody);
            var response = await _d365WebApiService.SendCreateRequestAsync(_appUserService.AZSystemAppUser, "ofm_payments", requestBody);
            if (!response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                _logger.LogError(CustomLogEvent.Process, "Failed to create a payment with the server error {responseBody}", responseBody.CleanLog());

                return ProcessResult.Failure(ProcessId, [responseBody], 0, 0).SimpleProcessResult;
            }
            return ProcessResult.Completed(ProcessId).SimpleProcessResult;
        }
        public async Task<JsonObject> RunProcessAsync(ID365AppUserService appUserService, ID365WebApiService d365WebApiService, ProcessParameter processParams)
        {
            var PSTZone = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");
            var pstTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, PSTZone);
            _logger.LogInformation(CustomLogEvent.Process, pstTime.ToString("yyyy-MM-dd HH:mm:ss") + "Process " + ProcessId + ": Begin to generate PaymentLine Process for ER: " + processParams.EnrolmentReportid);
            try
            {
                _processParams = processParams;
                var entitySetName = "ofm_payments";
                var monthlyECEWEReportString = await GetDataAsync();
                JsonArray? monthlyECEWEReportData = JsonSerializer.Deserialize<JsonArray>(monthlyECEWEReportString.Data);
                if (monthlyECEWEReportData is null || !monthlyECEWEReportData.Any())
                {
                    _logger.LogError(CustomLogEvent.Process, "Unable to retrieve the Monthly ECEWE Report record Id {MonthlyECEWEReport}", processParams!.MonthlyECEWEReport);
                    return ProcessResult.Completed(ProcessId).SimpleProcessResult;
                }
                JsonNode? monthlyECEWEReport = monthlyECEWEReportData.First();
                decimal grandTotalECEWE = 0, grandTotalSB = 0;
                grandTotalECEWE = ((decimal?)monthlyECEWEReport["ccof_we_subtotal"] ?? 0);

                grandTotalSB = ((decimal?)monthlyECEWEReport["ccof_sb_subtotal"] ?? 0);
                  
                switch ((int)processParams.programapproved)
                {
                    case 1:  // CCOF was approved in ER
                        await CreateSinglePayment(monthlyECEWEReport, pstTime, grandTotalECEWE, 1, 9);
                        await CreateSinglePayment(monthlyECEWEReport, pstTime, grandTotalSB, 1, 11);// 1, InvoiceLineNumber fix for CCOF Base Payment. 7 PaymentType CCOF
                        break; 


                    default:
                        _logger.LogError(CustomLogEvent.Process, "Unable to generate payments for Monthly ECEWE Report {ERid}. Invalid ApprovedType {programApproved}", processParams?.MonthlyECEWEReport, processParams?.programapproved);
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                var returnObject = ProcessResult.Failure(ProcessId, new String[] { "Critical error", ex.StackTrace }, 0, 0).ODProcessResult;
                return returnObject;
            }
            _logger.LogInformation(CustomLogEvent.Process, pstTime.ToString("yyyy-MM-dd HH:mm:ss") + "Process " + ProcessId + ": End Process for ER: " + processParams.MonthlyECEWEReport);
            return ProcessResult.Completed(ProcessId).SimpleProcessResult;
        }
    }
}