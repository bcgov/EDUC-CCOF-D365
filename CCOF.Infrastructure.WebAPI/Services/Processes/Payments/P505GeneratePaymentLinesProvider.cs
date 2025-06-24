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

namespace CCOF.Infrastructure.WebAPI.Services.Processes.Payments
{
    public class P505GeneratePaymentLinesProvider(IOptionsSnapshot<ExternalServices> bccasApiSettings, ID365AppUserService appUserService, ID365WebApiService d365WebApiService, ILoggerFactory loggerFactory, TimeProvider timeProvider) : ID365ProcessProvider
    {
        private readonly BCCASApi _BCCASApi = bccasApiSettings.Value.BCCASApi;
        private readonly ID365AppUserService _appUserService = appUserService;
        private readonly D365WebApi.ID365WebApiService _d365WebApiService = d365WebApiService;
        private readonly ILogger _logger = loggerFactory.CreateLogger(LogCategory.Process);

        private readonly TimeProvider _timeProvider = timeProvider;
        private ProcessParameter? _processParams;


        public Int16 ProcessId => Setup.Process.Payments.GeneratePaymentLinesId;
        public string ProcessName => Setup.Process.Payments.GeneratePaymentLinesName;

        public Task<ProcessData> GetDataAsync()
        {
            throw new NotImplementedException();
        }

        #region Data Queries

        public string applicationCCFRIs
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
        public async Task<ProcessData> GetBusinessClosuresDataAsync()
        {
            _logger.LogDebug(CustomLogEvent.Process, nameof(GetBusinessClosuresDataAsync));

            var response = await _d365WebApiService.SendRetrieveRequestAsync(_appUserService.AZSystemAppUser, applicationCCFRIs, false, 0, true);

            if (!response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                _logger.LogError(CustomLogEvent.Process, "Failed to query record information with the server error {responseBody}", responseBody.CleanLog());

                return await Task.FromResult(new ProcessData(string.Empty));
            }

            var jsonObject = await response.Content.ReadFromJsonAsync<JsonObject>();

            JsonNode d365Result = string.Empty;
            if (jsonObject?.TryGetPropertyValue("value", out var currentValue) == true)
            {
                if (currentValue?.AsArray().Count == 0)
                {
                    _logger.LogInformation(CustomLogEvent.Process, "No records found with query {requestUri}", applicationCCFRIs.CleanLog());
                }
                d365Result = currentValue!;
            }

            _logger.LogDebug(CustomLogEvent.Process, "Query Result {queryResult}", d365Result.ToString().CleanLog());

            return await Task.FromResult(new ProcessData(d365Result));
        }

        public async Task<JsonObject> RunProcessAsync(ID365AppUserService appUserService, ID365WebApiService d365WebApiService, ProcessParameter processParams)

        {
            #region Validation & Setup
            var entitySetName = "ccof_application_ccfri_closure";
            var payload2 = new JsonObject {
                                { "ccof_name", "test123" }
                             
                            };
            var requestBody2 = JsonSerializer.Serialize(payload2);
            var CreateResponse2 = await d365WebApiService.SendCreateRequestAsync(appUserService.AZSystemAppUser, entitySetName, requestBody2);
            _logger.LogTrace(CustomLogEvent.Process, "Start processing payments for the test.");

      
            #endregion
            return ProcessResult.Completed(ProcessId).SimpleProcessResult;
           
        }


    }


}