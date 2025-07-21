//using ECC.Core.DataContext;
using CCOF.Infrastructure.WebAPI.Extensions;
using CCOF.Infrastructure.WebAPI.Models;
using CCOF.Infrastructure.WebAPI.Services.AppUsers;
using CCOF.Infrastructure.WebAPI.Services.D365WebApi;
using System.Text.Json.Nodes;
using CCOF.Infrastructure.WebAPI.Messages;
using Microsoft.Extensions.Options;

namespace CCOF.Infrastructure.WebAPI.Services.Processes.Payments
{
    public class P400GenerateMonthlyEnrolmentReport : ID365ProcessProvider
    {
        private readonly ProcessSettings _processSettings;
        private readonly ID365AppUserService _appUserService;
        private readonly ID365WebApiService _d365webapiservice;
        private readonly ILogger _logger;
        private readonly TimeProvider _timeProvider;
        private ProcessData? _data;
        // private readonly BCCASApi _BCCASApi = bccasApiSettings.Value.BCCASApi;
        public P400GenerateMonthlyEnrolmentReport(IOptionsSnapshot<ProcessSettings> processSettings, ID365AppUserService appUserService, ID365WebApiService d365WebApiService, ILoggerFactory loggerFactory, TimeProvider timeProvider)
        {
            _processSettings = processSettings.Value;
            _appUserService = appUserService;
            _d365webapiservice = d365WebApiService;
            _logger = loggerFactory.CreateLogger(LogCategory.Process); ;
            _timeProvider = timeProvider;
        }


        public Int16 ProcessId => Setup.Process.MonthlyEnrolmentReports.CreateMonthlyEnrolmentReportsId;
        public string ProcessName => Setup.Process.MonthlyEnrolmentReports.CreateMonthlyEnrolmentReportsName;

        public async Task<ProcessData> GetDataAsync()
        {
            _logger.LogDebug(CustomLogEvent.Process, "Calling GetData of {nameof}", nameof(P400GenerateMonthlyEnrolmentReport));

            var response = await _d365webapiservice.SendRetrieveRequestAsync(_appUserService.AZSystemAppUser, RequestUri, isProcess: true);
            if (!response.IsSuccessStatusCode)
            {
                var responseBody = response.Content.ReadAsStringAsync();
                _logger.LogError(CustomLogEvent.Process, "Failed to query inactive requests with the server error {responseBody}", responseBody);

                return await Task.FromResult(new ProcessData(string.Empty));
            }

            var jsonObject = await response.Content.ReadFromJsonAsync<JsonObject>();

            JsonNode d365Result = string.Empty;
            if (jsonObject?.TryGetPropertyValue("value", out var currentValue) == true)
            {
                if (currentValue?.AsArray().Count == 0)
                {
                    _logger.LogInformation(CustomLogEvent.Process, "No inactive requests found");
                }
                d365Result = currentValue!;
            }

            _data = new ProcessData(d365Result);

            _logger.LogDebug(CustomLogEvent.Process, "Query Result {_data}", _data.Data.ToJsonString());

            return await Task.FromResult(_data);
        }

        #region Data Queries

        public string RequestUri
        {
            get
            {
                // For reference only
                var fetchXml = $$"""
                    <fetch version="1.0" output-format="xml-platform" mapping="logical" distinct="true">
                      <entity name="ccof_application_ccfri_closure">
                        <attribute name="ccof_name" />
                        <attribute name="ccof_application_ccfri_closureid" />
                        <attribute name="ccof_startdate" />
                        <attribute name="ccof_enddate" />
                        <attribute name="ccof_is_full_closure" />
                        <attribute name="ccof_totalworkdays" />
                        <attribute name="ccof_totaldays" />
                        <attribute name="ccof_paidclosure" />
                        <attribute name="ccof_closure_status" />
                        <attribute name="ccof_facilityinfo" />
                        <attribute name="ccof_closure_type" />
                        <filter type="and">
                          <condition attribute="ccof_approved_as" operator="eq" value="100000000" />
                          <condition attribute="ccof_facilityinfo" operator="eq" value="e4077ebe-0310-f011-9989-000d3a09ed17" />
                          <condition attribute="ccof_closure_status" operator="eq" value="100000003" />
                          <condition attribute="ccof_program_year" operator="eq" value="fdc2fce3-d1a2-ef11-8a6a-000d3af474a4" uiname="2025-26 FY" uitype="ccof_program_year" />
                        </filter>
                      </entity>
                    </fetch>
                    """;

                var requestUri = $"""
                         ccof_application_ccfri_closures?$select=ccof_name,ccof_application_ccfri_closureid,ccof_startdate,ccof_enddate,ccof_is_full_closure,ccof_totalworkdays,ccof_totaldays,ccof_paidclosure,ccof_closure_status,_ccof_facilityinfo_value,ccof_closure_type&$filter=(ccof_approved_as eq 100000000 and ccof_closure_status eq 100000003)
                         """;

                return requestUri;
            }
        }


        #endregion

        public async Task<JsonObject> RunProcessAsync(ID365AppUserService appUserService, ID365WebApiService d365WebApiService, ProcessParameter processParams)

        {
            #region Validation & Setup
            _logger.LogTrace(CustomLogEvent.Process, "Start creating the batch request for Monthly Enrolment Report.");
            var localData = await GetDataAsync();
            if (localData.Data.AsArray().Count == 0)
            {
                _logger.LogInformation(CustomLogEvent.Process, "No facility found for which Monthly Enrolment report needs to be created.");
                return ProcessResult.Completed(ProcessId).SimpleProcessResult;
            }

            #endregion

            List<HttpRequestMessage> requests = new() { };
            foreach (var request in localData.Data.AsArray())
            {
                var body = new JsonObject()
                {
                    //["attributeName"] = value,
                    //["attributeName"] = value
                };

                requests.Add(new CreateRequest("ccof_monthlyenrollmentreport", body));
            }

            var batchResult = await d365WebApiService.SendBatchMessageAsync(appUserService.AZSystemAppUser, requests, null);

            if (batchResult.Errors.Any())
            {
                var result = ProcessResult.Failure(ProcessId, batchResult.Errors, batchResult.TotalProcessed, batchResult.TotalRecords);

                _logger.LogError(CustomLogEvent.Process, "Create Monthly Enrolment Report process finished with an error {error}", JsonValue.Create(result)!.ToJsonString());

                return result.SimpleProcessResult;
            }

            return ProcessResult.Completed(ProcessId).SimpleProcessResult;
           
        }


    }


}