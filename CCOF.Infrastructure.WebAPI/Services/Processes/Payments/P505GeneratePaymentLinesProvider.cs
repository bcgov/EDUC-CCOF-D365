//using ECC.Core.DataContext;
using System.Text.Json.Serialization;
using CCOF.Infrastructure.WebAPI.Extensions;
using CCOF.Infrastructure.WebAPI.Models;

using CCOF.Infrastructure.WebAPI.Services.AppUsers;
using CCOF.Infrastructure.WebAPI.Services.D365WebApi;
using System.Net;
using System.Text.Json.Nodes;
using System.Text.Json;
using CCOF.Infrastructure.WebAPI.Services.Processes;
using CCOF.Infrastructure.WebAPI.Messages;
using Microsoft.Extensions.Options;
using System;
using CCOF.Infrastructure.WebAPI.Services.D365WebAPI;
using CCOF.Core.DataContext;
using System.Drawing.Text;

namespace CCOF.Infrastructure.WebAPI.Services.Processes.Payments
{
    public class P505GeneratePaymentLinesProvider(IOptionsSnapshot<ExternalServices> bccasApiSettings, ID365AppUserService appUserService, ID365WebApiService d365WebApiService, ILoggerFactory loggerFactory) : ID365ProcessProvider
    {
        private readonly BCCASApi _BCCASApi = bccasApiSettings.Value.BCCASApi;
        private readonly ID365AppUserService _appUserService = appUserService;
        private readonly ID365WebApiService _d365WebApiService = d365WebApiService;
        private readonly ILogger _logger = loggerFactory.CreateLogger(LogCategory.Process);
        private ProcessParameter? _processParams;
        public Int16 ProcessId => Setup.Process.Payments.GeneratePaymentLinesId;
        public string ProcessName => Setup.Process.Payments.GeneratePaymentLinesName;
        #region Data Queries
        public string BusinessClosuresRequestUri
        { // ofm_holiday_type" operator="eq" value="1"  Standard for CCOF. 2 for OFM
            get
            {
                var fetchXml = $$"""
                    <fetch>
                      <entity name="ofm_stat_holiday">
                        <attribute name="ofm_date_observed" />
                        <attribute name="ofm_holiday_type" />
                        <attribute name="ofm_stat_holidayid" />
                        <filter>
                          <condition attribute="ofm_holiday_type" operator="eq" value="1" />
                        </filter>
                      </entity>
                    </fetch>
                    """;

                var requestUri = $"""
                         ofm_stat_holidaies?fetchXml={WebUtility.UrlEncode(fetchXml)}
                         """;

                return requestUri;
            }
        }
        public string EnrolmentReportPaymentUri
        {
            get
            {
                var fetchXml = $$"""
                    <fetch version="1.0" output-format="xml-platform" mapping="logical" distinct="false">
                      <entity name="ccof_monthlyenrollmentreport">
                        <attribute name="ccof_ccfri_external_status" />
                        <attribute name="ccof_ccfri_internal_status" />
                        <attribute name="ccof_ccfri_verification" />
                        <attribute name="ccof_ccof_base_verification" />
                        <attribute name="ccof_ccof_external_status" />
                        <attribute name="ccof_ccof_internal_status" />
                        <attribute name="ccof_facility" />
                        <attribute name="ccof_grandtotalbase" />
                        <attribute name="ccof_grandtotalccfri" />
                        <attribute name="ccof_grandtotalccfriprovider" />
                        <attribute name="ccof_month" />
                        <attribute name="ccof_monthlyenrollmentreportid" />
                        <attribute name="ccof_organization" />
                        <attribute name="ccof_programyear" />
                        <attribute name="ccof_reportversion" />
                        <attribute name="ccof_year" />
                        <attribute name="statecode" />
                        <attribute name="statuscode" />
                        <attribute name="ccof_ccfri_approved_date" />
                        <attribute name="ccof_ccof_approved_date" />
                        <link-entity name="account" from="accountid" to="ccof_facility" link-type="inner" alias="facility">
                          <attribute name="name" />
                          <attribute name="accountnumber" />
                    </link-entity>
                        <filter>
                          <condition attribute="ccof_monthlyenrollmentreportid" operator="eq" value="{{_processParams.EnrolmentReportid.ToString()}}" />
                        </filter>
                      </entity>
                    </fetch>
                    """;
                var requestUri = $"""
                         ccof_monthlyenrollmentreports?fetchXml={WebUtility.UrlEncode(fetchXml)}
                         """;
                return requestUri;
            }
        }
        #endregion
        public async Task<ProcessData> GetBusinessClosuresDataAsync()
        {
            _logger.LogDebug(CustomLogEvent.Process, nameof(GetBusinessClosuresDataAsync));

            var response = await _d365WebApiService.SendRetrieveRequestAsync(_appUserService.AZSystemAppUser, BusinessClosuresRequestUri, false, 0, true);

            if (!response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                _logger.LogError(CustomLogEvent.Process, "Failed to query Funding record information with the server error {responseBody}", responseBody.CleanLog());

                return await Task.FromResult(new ProcessData(string.Empty));
            }

            var jsonObject = await response.Content.ReadFromJsonAsync<JsonObject>();

            JsonNode d365Result = string.Empty;
            if (jsonObject?.TryGetPropertyValue("value", out var currentValue) == true)
            {
                if (currentValue?.AsArray().Count == 0)
                {
                    _logger.LogInformation(CustomLogEvent.Process, "No records found with query {requestUri}", BusinessClosuresRequestUri.CleanLog());
                }
                d365Result = currentValue!;
            }

            _logger.LogDebug(CustomLogEvent.Process, "Query Result {queryResult}", d365Result.ToString().CleanLog());

            return await Task.FromResult(new ProcessData(d365Result));
        }
        public async Task<ProcessData> GetDataAsync()
        {
            _logger.LogDebug(CustomLogEvent.Process, "GetDataAsync");

            var response = await _d365WebApiService.SendRetrieveRequestAsync(_appUserService.AZSystemAppUser, EnrolmentReportPaymentUri);

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
                    _logger.LogInformation(CustomLogEvent.Process, "No records found with query {requestUri}", EnrolmentReportPaymentUri.CleanLog());
                }
                d365Result = currentValue!;
            }

            _logger.LogDebug(CustomLogEvent.Process, "Query Result {queryResult}", d365Result.ToString().CleanLog());

            return await Task.FromResult(new ProcessData(d365Result));
        }
        private async Task<JsonObject> CreateSinglePayment(JsonNode enrolmentReport,
                                                                           DateTime paymentDate,
                                                                           decimal? totalAmount,
                                                                           ProcessParameter processParams,
                                                                           List<DateTime> holidaysList)
        {
            DateTime invoiceDate = paymentDate.GetPreviousBusinessDay(holidaysList);
            DateTime invoiceReceivedDate = invoiceDate.AddBusinessDays(_BCCASApi.PayableInDays, holidaysList);
            DateTime effectiveDate = invoiceDate;
            string paymentType = (int)processParams.programapproved switch
            {
                7 => "CCOF",
                8 => "CCFRI",
                9 => "CCFRI Provider",
                _ => "Unknown"
            };
            int invoiceLineNumber = (int)processParams.programapproved switch
            {
                7 => 1,
                8 => 2,
                9 => 3,
                _ => 999999
            };
            var payload = new JsonObject()
                        {
                            { "ofm_invoice_line_number", invoiceLineNumber},
                            { "ofm_amount", totalAmount},
                            { "ofm_payment_type", (int) processParams.programapproved},
                            { "ccof_monthly_enrollment_report@odata.bind",$"/ccof_monthlyenrollmentreports({enrolmentReport["ccof_monthlyenrollmentreportid"]})" },
                            { "ofm_invoice_date", invoiceDate.ToString("yyyy-MM-dd") },
                            { "ofm_invoice_received_date", invoiceReceivedDate.ToString("yyyy-MM-dd")},
                            { "ofm_effective_date", effectiveDate.ToString("yyyy-MM-dd")},
                            { "ccof_program_year@odata.bind",$"/ccof_program_years({enrolmentReport["_ccof_programyear_value"]})" },
                            { "statuscode",4  }, //Approved for Payment in PaymentLine table
                            { "ofm_facility@odata.bind", $"/accounts({enrolmentReport["_ccof_facility_value"]})" },
                            { "ofm_organization@odata.bind", $"/accounts({enrolmentReport["_ccof_organization_value"]})" },
                            { "ofm_description", $"{enrolmentReport["facility.name"] +" " +enrolmentReport["ccof_month"]+"/"+enrolmentReport["ccof_year"]+" " +paymentType}" },
                        };
            var requestBody = JsonSerializer.Serialize(payload);
            var response = await _d365WebApiService.SendCreateRequestAsync(_appUserService.AZSystemAppUser, "ofm_payments", requestBody);
            if (!response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                _logger.LogError(CustomLogEvent.Process, "Failed to create a payment with the server error {responseBody}. ProcessParam {param}", responseBody.CleanLog(), JsonValue.Create(processParams)?.ToString());

                return ProcessResult.Failure(ProcessId, [responseBody], 0, 0).SimpleProcessResult;
            }

            return ProcessResult.Completed(ProcessId).SimpleProcessResult;
        }
        public async Task<JsonObject> RunProcessAsync(ID365AppUserService appUserService, ID365WebApiService d365WebApiService, ProcessParameter processParams)
        {
            var PSTZone = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");
            var pstTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, PSTZone);
            _logger.LogInformation(CustomLogEvent.Process, pstTime.ToString("yyyy-MM-dd HH:mm:ss") + "Process " + ProcessId + ": Begin to generate PaymentLine Process");
            try
            {
                _processParams = processParams;
                var entitySetName = "ofm_payments";
                var enrolmentReportString = await GetDataAsync();
                JsonArray? enrolmentReportData = JsonSerializer.Deserialize<JsonArray>(enrolmentReportString.Data);
                if (enrolmentReportData is null || !enrolmentReportData.Any())
                {
                    _logger.LogError(CustomLogEvent.Process, "Unable to retrieve the Monthly Enrolment Report record Id {enrolmentReportId}", processParams!.EnrolmentReportid);
                    return ProcessResult.Completed(ProcessId).SimpleProcessResult;
                }
                JsonNode? enrolmentReport = enrolmentReportData.First();
                var businessClosuresData = await GetBusinessClosuresDataAsync();
                var closures = JsonSerializer.Deserialize<JsonArray>(businessClosuresData.Data.ToString());
                List<DateTime> holidaysList = closures!.Select(closure => (DateTime)closure["ofm_date_observed"]).ToList();
                switch ((int)processParams.programapproved)
                {
                    case 7:  // ofm_payment_type CCOF
                        await CreateSinglePayment(enrolmentReport, (DateTime)enrolmentReport["ccof_ccof_approved_date"], (decimal)enrolmentReport["ccof_grandtotalbase"], processParams!, holidaysList);
                        break;
                    case 8: // ofm_payment_type CCFRI
                        await CreateSinglePayment(enrolmentReport, (DateTime)enrolmentReport["ccof_ccfri_approved_date"], ((decimal?)(enrolmentReport["ccof_grandtotalccfriprovider"]) ?? 0) + ((decimal?)(enrolmentReport["ccof_grandtotalccfri"]) ?? 0), processParams!, holidaysList);
                        break;
                    default:
                        _logger.LogError(CustomLogEvent.Process, "Unable to generate payments for Erolment Report {ERid}. Invalid ApprovedType {programApproved}", processParams?.EnrolmentReportid, processParams?.programapproved);
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                var returnObject = ProcessResult.Failure(ProcessId, new String[] { "Critical error", ex.StackTrace }, 0, 0).ODProcessResult;
                return returnObject;
            }
            _logger.LogInformation(CustomLogEvent.Process, pstTime.ToString("yyyy-MM-dd HH:mm:ss") + "Process " + ProcessId + ": End Process.");
            return ProcessResult.Completed(ProcessId).SimpleProcessResult;
        }
    }
}