//using ECC.Core.DataContext;
using CCOF.Infrastructure.WebAPI.Extensions;
using CCOF.Infrastructure.WebAPI.Models;
using CCOF.Infrastructure.WebAPI.Services.AppUsers;
using CCOF.Infrastructure.WebAPI.Services.D365WebApi;
using System.Text.Json.Nodes;
using CCOF.Infrastructure.WebAPI.Messages;
using Microsoft.Extensions.Options;
using System.Net;
using CCOF.Infrastructure.WebAPI.Services.D365WebAPI;
using System.Text.Json;

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
        private ProcessParameter? _processParams;

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
            throw new NotImplementedException();

        }

        #region Data Queries
        public string RateRequestUri
        {
            get
            {
                var fetchXml = $$"""
                        <fetch>
                          <entity name="ccof_rate">
                            <filter>
                              <condition attribute="statecode" operator="eq" value="0" />
                            </filter>
                          </entity>
                        </fetch>
                        """;
                var requestUri = $"ccof_rates?fetchXml=" + WebUtility.UrlEncode(fetchXml);
                return requestUri;
            }
        }
        public string MonthlyBusinessDayRequestUri
        {
            get
            {
                var fetchXml = $$"""
                        <fetch>
                          <entity name="ccof_monthlybusinessday">
                            <attribute name="ccof_businessday" />
                            <attribute name="ccof_month" />
                            <attribute name="ccof_name" />
                            <filter>
                              <condition attribute="ccof_month" operator="eq" value="{{_processParams.InitialEnrolmentReport.Month}}" />
                              <condition attribute="ccof_programyear" operator="eq" value="{{_processParams.InitialEnrolmentReport.ProgramYearId}}" />
                              <condition attribute="statecode" operator="eq" value="0" />
                            </filter>
                          </entity>
                        </fetch>
                        """;
                var requestUri = $"ccof_monthlybusinessdaies?fetchXml=" + WebUtility.UrlEncode(fetchXml);
                return requestUri;

            }
        }
        public string StatutoryDayRequestUri
        {
            get
            {
                // fetchxml for reference
                var fetchXml = $$"""
                        <fetch>
                          <entity name="ofm_stat_holiday">
                            <filter>
                              <condition attribute="statecode" operator="eq" value="0" />
                            </filter>
                          </entity>
                        </fetch>
                        """;
                var requestUri = $"ofm_stat_holidaies?fetchXml=" + WebUtility.UrlEncode(fetchXml);
                return requestUri;
            }
        }
        public string ApprovedParentFeeRequestUri
        {
            get
            {
                var facilityValuesXml = string.Join(
            "",
            _processParams.InitialEnrolmentReport.FacilityGuid.Select(guid => $"<value>{guid}</value>")
        );
                // fetch xml doesn't support binary data type
                var fetchXml = $$"""
                        <fetch>
                          <entity name="ccof_parent_fees">
                            <attribute name="ccof_apr" />
                            <attribute name="ccof_aug" />
                            <attribute name="ccof_availability" />
                            <attribute name="ccof_childcarecategory" />
                            <attribute name="ccof_dec" />
                            <attribute name="ccof_facility" />
                            <attribute name="ccof_feb" />
                            <attribute name="ccof_frequency" />
                            <attribute name="ccof_jan" />
                            <attribute name="ccof_jul" />
                            <attribute name="ccof_jun" />
                            <attribute name="ccof_mar" />
                            <attribute name="ccof_may" />
                            <attribute name="ccof_name" />
                            <attribute name="ccof_nov" />
                            <attribute name="ccof_oct" />
                            <attribute name="ccof_programyear" />
                            <attribute name="ccof_sep" />
                            <attribute name="ccof_type" />
                            <attribute name="statecode" />
                            <attribute name="statuscode" />
                            <filter>
                              <condition attribute="ccof_facility" operator="in">
                                {{facilityValuesXml}}
                              </condition>
                              <condition attribute="statecode" operator="eq" value="0" />
                              <condition attribute="statuscode" operator="eq" value="1" />
                              <condition attribute="ccof_type" operator="eq" value="1" />
                              <condition attribute="ccof_programyear" operator="eq" value="{{_processParams.InitialEnrolmentReport.ProgramYearId}}" />
                            </filter>
                            <link-entity name="ccof_childcare_category" from="ccof_childcare_categoryid" to="ccof_childcarecategory" link-type="inner" alias="childcareCategory">
                              <attribute name="ccof_childcarecategorynumber" />
                              <attribute name="ccof_name" />
                              <order attribute="ccof_childcarecategorynumber" />
                            </link-entity>
                          </entity>
                        </fetch>
                        """;
                var requestUri = $"ccof_parent_feeses?fetchXml=" + WebUtility.UrlEncode(fetchXml);
                return requestUri;
            }
        }
        public string OrgRequestUri
        {
            get
            {
                var facilityValuesXml = string.Join(
            "",
            _processParams.InitialEnrolmentReport.FacilityGuid.Select(guid => $"<value>{guid}</value>")
        );
                // fetch xml doesn't support binary data type
                var fetchXml = $$"""
                        <fetch>
                          <entity name="account">
                            <attribute name="name" />
                            <filter>
                              <condition attribute="accountid" operator="in">
                                {{facilityValuesXml}}
                              </condition>
                            </filter>
                            <link-entity name="account" from="accountid" to="parentaccountid" link-type="inner" alias="org">
                              <attribute name="accountnumber" />
                              <attribute name="accountid" />
                            </link-entity>
                          </entity>
                        </fetch>
                        """;
                var requestUri = $"accounts?fetchXml=" + WebUtility.UrlEncode(fetchXml);
                return requestUri;
            }
        }
        public string FacilityLicenceRequestUri
        {
            get
            {
                var facilityValuesXml = string.Join(
            "",
            _processParams.InitialEnrolmentReport.FacilityGuid.Select(guid => $"<value>{guid}</value>")
        );
                // fetch xml doesn't support binary data type
                var fetchXml = $$"""
                        <fetch>
                          <entity name="ccof_facility_licenses">
                            <attribute name="ccof_facility" />
                            <attribute name="ccof_name" />
                            <attribute name="statecode" />
                            <attribute name="statuscode" />
                            <attribute name="ccof_licensecategory" />
                            <filter>
                              <condition attribute="ccof_facility" operator="in">
                                {{facilityValuesXml}}
                              </condition>
                              <condition attribute="statecode" operator="eq" value="0" />
                            </filter>
                            <link-entity name="ccof_license_category" from="ccof_license_categoryid" to="ccof_licensecategory" link-type="inner" alias="licenceCategory">
                              <attribute name="ccof_categorynumber" />
                              <attribute name="ccof_name" />
                              <attribute name="ccof_providertype" />
                              <filter type="or">
                                <condition attribute="ccof_categorynumber" operator="eq" value="6" />
                                <condition attribute="ccof_categorynumber" operator="eq" value="5" />
                              </filter>
                            </link-entity>
                          </entity>
                        </fetch>
                        """;
                var requestUri = $"ccof_facility_licenseses?fetchXml=" + WebUtility.UrlEncode(fetchXml);
                return requestUri;
            }
        }
        public string monthLogicalNameString = """
        [
          { "id": 1, "enrolmentMonth": 1, "monthNameinApprovedParentFee": "ccof_jan" },
          { "id": 2, "enrolmentMonth": 2, "monthNameinApprovedParentFee": "ccof_feb" },
          { "id": 3, "enrolmentMonth": 3, "monthNameinApprovedParentFee": "ccof_mar" },
          { "id": 4, "enrolmentMonth": 4, "monthNameinApprovedParentFee": "ccof_apr" },
          { "id": 5, "enrolmentMonth": 5, "monthNameinApprovedParentFee": "ccof_may" },
          { "id": 6, "enrolmentMonth": 6, "monthNameinApprovedParentFee": "ccof_jun" },
          { "id": 7, "enrolmentMonth": 7, "monthNameinApprovedParentFee": "ccof_jul" },
          { "id": 8, "enrolmentMonth": 8, "monthNameinApprovedParentFee": "ccof_aug" },
          { "id": 9, "enrolmentMonth": 9, "monthNameinApprovedParentFee": "ccof_sep" },
          { "id": 10, "enrolmentMonth": 10, "monthNameinApprovedParentFee": "ccof_oct" },
          { "id": 11, "enrolmentMonth": 11, "monthNameinApprovedParentFee": "ccof_nov" },
          { "id": 12, "enrolmentMonth": 12, "monthNameinApprovedParentFee": "ccof_dec" }
        ]
        """;
        #endregion
        public async Task<List<JsonNode>> FetchAllRecordsFromCRMAsync(string requestUri)
        {
            _logger.LogDebug(CustomLogEvent.Process, "Getting records with query {requestUri}", requestUri.CleanLog());
            var allRecords = new List<JsonNode>();  // List to accumulate all records
            string nextPageLink = requestUri;  // Initial request URI
            do
            {
                // 5000 is limit number can retrieve from crm
                var response = await _d365webapiservice.SendRetrieveRequestAsync(_appUserService.AZSystemAppUser, nextPageLink, false, 5000, isProcess: false);
                if (!response.IsSuccessStatusCode)
                {
                    var responseBody = await response.Content.ReadAsStringAsync();
                    _logger.LogError(CustomLogEvent.Process, "Failed to query records with server error {responseBody}", responseBody.CleanLog());
                    var returnJsonNodeList = new List<JsonNode>();
                    returnJsonNodeList.Add(responseBody);
                    return returnJsonNodeList;
                    // null;
                }
                var jsonObject = await response.Content.ReadFromJsonAsync<JsonObject>();
                JsonNode currentBatch = string.Empty;
                if (jsonObject?.TryGetPropertyValue("value", out var currentValue) == true)
                {
                    if (currentValue?.AsArray().Count == 0)
                    {
                        _logger.LogInformation(CustomLogEvent.Process, "No more records found with query {nextPageLink}", nextPageLink.CleanLog());
                        break;  // Exit the loop if no more records
                    }
                    currentBatch = currentValue!;
                    allRecords.AddRange(currentBatch.AsArray());  // Add current batch to the list
                }
                _logger.LogDebug(CustomLogEvent.Process, "Fetched {batchSize} records. Total records so far: {totalRecords}", currentBatch.AsArray().Count, allRecords.Count);

                // Check if there's a next link in the response for pagination
                nextPageLink = null;
                if (jsonObject?.TryGetPropertyValue("@odata.nextLink", out var nextLinkValue) == true)
                {
                    nextPageLink = nextLinkValue.ToString();
                }
            }
            while (!string.IsNullOrEmpty(nextPageLink));

            _logger.LogDebug(CustomLogEvent.Process, "Total records fetched: {totalRecords}", allRecords.Count);
            return allRecords;
        }

        public static List<JsonNode> GenerateDailyEnrolment(int year, int month, string statutoryHolidaysJson)
        {
            // Parse statutory holiday dates
            var holidayDates = new HashSet<DateTime>();
            using var doc = JsonDocument.Parse(statutoryHolidaysJson);
            foreach (var element in doc.RootElement.EnumerateArray())
            {
                if (element.TryGetProperty("ofm_date_observed", out var dateProp) &&
                    dateProp.ValueKind == JsonValueKind.String &&
                    DateTime.TryParse(dateProp.GetString(), out var parsedDate))
                {
                    holidayDates.Add(parsedDate.Date);
                }
            }

            var result = new List<JsonNode>();
            int daysInMonth = DateTime.DaysInMonth(year, month);

            for (int day = 1; day <= daysInMonth; day++)
            {
                var date = new DateTime(year, month, day);
                int dayType;

                if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
                {
                    dayType = 100000001; // Weekend
                }
                else if (holidayDates.Contains(date))
                {
                    dayType = 100000002; // Statutory holiday
                }
                else
                {
                    dayType = 100000000; // Regular weekday
                }

                var jsonDay = new JsonObject
                {
                    ["ccof_day"] = day,
                    ["ccof_daytype"] = dayType
                };

                result.Add(jsonDay);
            }

            return result;
        }
        public JsonObject CalculateDailyCCFRIRate(JsonObject approvedParentFee, JsonObject CCFRIMax, JsonObject CCFRIMin, bool feeFloorExempt, int monthlyBusinessDay, int providerType)
        {
            return new JsonObject()
            {
                ["ccof_dailyccfrirateless0to18"] = 1,
                ["ccof_dailyccfrirateover0to18"] = 1,
                ["ccof_dailyccfrirateless18to36"] = 1,
                ["ccof_dailyccfrirateover18to36"] = 1,
                ["ccof_dailyccfrirateless3yk"] = 1,
                ["ccof_dailyccfrirateover3yk"] = 1,
                ["ccof_dailccfriratelessoosck"] = 1,
                ["ccof_dailyccfrirateoveroosck"] = 1,
                ["ccof_dailyccfriratelessooscg"] = 1,
                ["ccof_dailyccfrirateoverooscg"] = 1,
                ["ccof_dailyccfriratelesspre"] = 1,

            };
        }

        public async Task<JsonObject> RunProcessAsync(ID365AppUserService appUserService, ID365WebApiService d365WebApiService, ProcessParameter processParams)
        {
            _logger.LogInformation(CustomLogEvent.Process, "Beging to Initial ER process");
            try
            {
                _processParams = processParams;
                int businessDay = 0;
                var entitySetName = "ccof_monthlyenrollmentreports";
                // for Approved Parent fee
                var MonthLogicalNameTemp = JsonNode.Parse(monthLogicalNameString)?.AsArray() ?? throw new Exception("Invalid JSON");
                List<JsonNode> MonthLogicalNameArray = MonthLogicalNameTemp.Select(node => node!).ToList();
                string MonthLogicalName = MonthLogicalNameArray.FirstOrDefault(node => node["enrolmentMonth"].GetValue<int>() == _processParams.InitialEnrolmentReport.Month)["monthNameinApprovedParentFee"]?.GetValue<string>();
                _processParams = processParams;
                // Retrive all Rates
                List<JsonNode> rate = await FetchAllRecordsFromCRMAsync(RateRequestUri);
                //Retrive all statuary days
                List<JsonNode> statutoryDay = await FetchAllRecordsFromCRMAsync(StatutoryDayRequestUri);
                List<JsonNode> dailyEnrolment = GenerateDailyEnrolment(int.Parse(_processParams.InitialEnrolmentReport.Year), _processParams.InitialEnrolmentReport.Month ?? 1, JsonSerializer.Serialize(statutoryDay));
                // Retrive all Monthly Business days
                List<JsonNode> monthlyBusinessDay = await FetchAllRecordsFromCRMAsync(MonthlyBusinessDayRequestUri);
                if (monthlyBusinessDay.Count > 0)
                {
                    businessDay = monthlyBusinessDay.FirstOrDefault(node => node?["ccof_businessday"] != null)["ccof_businessday"]!.GetValue<int>();
                }
                else
                {
                    throw new InvalidOperationException($"Business days is missing.");
                }
                // Retrive all Approved Parent Fee for all Facilities
                List<JsonNode> ApprovedParentFee = await FetchAllRecordsFromCRMAsync(ApprovedParentFeeRequestUri);
                List<JsonNode> orgInfo = await FetchAllRecordsFromCRMAsync(OrgRequestUri);
                List<JsonNode> facilityLicence = await FetchAllRecordsFromCRMAsync(FacilityLicenceRequestUri);

                // Batch processing
                // int batchSize = 1000;
                int batchSize = 500;
                for (int i = 0; i < _processParams.InitialEnrolmentReport.FacilityGuid.Count(); i += batchSize)
                {
                    List<HttpRequestMessage> createEnrolmentReportRequests = [];
                    var batch = _processParams.InitialEnrolmentReport.FacilityGuid.Skip(i).Take(batchSize).ToList();
                    foreach (var record in batch)
                    {
                        int providerType = 100000000;  // Group
                        // Fee Floor Exempt
                        Boolean feeFloorExempt = false;
                        var org = orgInfo.FirstOrDefault(node => node?["accountid"]?.GetValue<string>() == record);
                        if (org != null)
                        {
                            var accountNumber = org["org.accountnumber"]?.GetValue<string>();

                            if (!string.IsNullOrEmpty(accountNumber) &&
                                accountNumber.StartsWith("G", StringComparison.OrdinalIgnoreCase))
                            {
                                providerType = 100000000; // Group
                            }
                            else
                            {
                                providerType = 100000001; // Family
                            }
                        }
                        else
                        {
                            throw new InvalidOperationException($"Organization with accountid '{record}' not found.");
                        }
                        bool IHMALicenceExist = facilityLicence.Any(node => node?["_ccof_facility_value"]?.GetValue<string>() == record);
                        var ccofBaseRate = rate.FirstOrDefault(node => node?["ccof_providertype"]?.GetValue<int>() == providerType &&
                                                               node?["ccof_ratetype"]?.GetValue<int>() == (int)(IHMALicenceExist ? 100000001 : 100000000)); // 100000001 IHMA Base Funding;  100000000 Base Funding
                        var ccfriProviderPaymentRate = rate.FirstOrDefault(node => node?["ccof_providertype"]?.GetValue<int>() == providerType &&
                                                               node?["ccof_ratetype"]?.GetValue<int>() == (int)(IHMALicenceExist ? 100000003 : 100000002)); // 100000003 IHMA Provider Payment Rate;100000002 CCFRI Provider Payment;
                        var ccfriMax = rate.FirstOrDefault(node => node?["ccof_providertype"]?.GetValue<int>() == providerType &&
                                                                   node?["ccof_ratetype"]?.GetValue<int>() == 100000004 &&
                                                                   node?["ccof_businessday"]?.GetValue<int>() == businessDay); //100000004 CCFRI Max;
                        var ccfriMin = rate.FirstOrDefault(node => node?["ccof_providertype"]?.GetValue<int>() == providerType &&
                                                                   node?["ccof_ratetype"]?.GetValue<int>() == 100000005 &&
                                                                   node?["ccof_businessday"]?.GetValue<int>() == businessDay); // 100000005 CCFRI Min
                        var approvedParentfee0to18 = ApprovedParentFee.FirstOrDefault(node => node?["childcareCategory.ccof_childcarecategorynumber"].GetValue<int>() == 1 &&
                                                                        node?["_ccof_facility_value"].GetValue<string>() == record);
                        var approvedParentfee18to36 = ApprovedParentFee.FirstOrDefault(node => node?["childcareCategory.ccof_childcarecategorynumber"].GetValue<int>() == 2 &&
                                                node?["_ccof_facility_value"].GetValue<string>() == record);
                        var approvedParentfee3YK = ApprovedParentFee.FirstOrDefault(node => node?["childcareCategory.ccof_childcarecategorynumber"].GetValue<int>() == 3 &&
                                                node?["_ccof_facility_value"].GetValue<string>() == record);
                        var approvedParentfeeOOSCK = ApprovedParentFee.FirstOrDefault(node => node?["childcareCategory.ccof_childcarecategorynumber"].GetValue<int>() == 4 &&
                                                node?["_ccof_facility_value"].GetValue<string>() == record);
                        var approvedParentfeeOOSCG = ApprovedParentFee.FirstOrDefault(node => node?["childcareCategory.ccof_childcarecategorynumber"].GetValue<int>() == 5 &&
                                                node?["_ccof_facility_value"].GetValue<string>() == record);
                        var approvedParentfeePre = ApprovedParentFee.FirstOrDefault(node => node?["childcareCategory.ccof_childcarecategorynumber"].GetValue<int>() == 6 &&
                                                node?["_ccof_facility_value"].GetValue<string>() == record);
                        var approvedParentFee = new JsonObject()
                        {
                            ["ccof_approvedparentfee0to18"] = (approvedParentfee0to18 == null || approvedParentfee0to18[MonthLogicalName] == null ||
                                                                    approvedParentfee0to18[MonthLogicalName].GetValue<decimal>() == 0)
                                                                    ? null : approvedParentfee0to18[MonthLogicalName].GetValue<decimal>(),
                            ["ccof_approvedparentfee18to36"] = (approvedParentfee18to36 == null || approvedParentfee18to36[MonthLogicalName] == null ||
                                                                    approvedParentfee18to36[MonthLogicalName].GetValue<decimal>() == 0)
                                                                    ? null : approvedParentfee18to36[MonthLogicalName].GetValue<decimal>(),
                            ["ccof_approvedparentfee3yk"] = (approvedParentfee3YK == null || approvedParentfee3YK[MonthLogicalName] == null ||
                                                                    approvedParentfee3YK[MonthLogicalName].GetValue<decimal>() == 0)
                                                                    ? null : approvedParentfee3YK[MonthLogicalName].GetValue<decimal>(),
                            ["ccof_approvedparentfeeoosck"] = (approvedParentfeeOOSCK == null || approvedParentfeeOOSCK[MonthLogicalName] == null ||
                                                                    approvedParentfeeOOSCK[MonthLogicalName].GetValue<decimal>() == 0)
                                                                    ? null : approvedParentfee18to36[MonthLogicalName].GetValue<decimal>(),
                            ["ccof_approvedparentfeeooscg"] = (approvedParentfeeOOSCG == null || approvedParentfeeOOSCG[MonthLogicalName] == null ||
                                                                    approvedParentfeeOOSCG[MonthLogicalName].GetValue<decimal>() == 0)
                                                                    ? null : approvedParentfeeOOSCK[MonthLogicalName].GetValue<decimal>(),
                            ["ccof_approvedparentfeepre"] = (approvedParentfeePre == null || approvedParentfeePre[MonthLogicalName] == null ||
                                                                    approvedParentfeePre[MonthLogicalName].GetValue<decimal>() == 0)
                                                                    ? null : approvedParentfeePre[MonthLogicalName].GetValue<decimal>(),
                            ["ccof_approvedparentfeefrequency0to18"] = (approvedParentfee0to18 == null || approvedParentfee0to18[MonthLogicalName] == null ||
                                                                    approvedParentfee0to18[MonthLogicalName].GetValue<decimal>() == 0)
                                                                    ? null : approvedParentfee0to18["ccof_frequency"].GetValue<int>(),
                            ["ccof_approvedparentfeefrequency18to36"] = (approvedParentfee18to36 == null || approvedParentfee18to36[MonthLogicalName] == null ||
                                                                    approvedParentfee18to36[MonthLogicalName].GetValue<decimal>() == 0)
                                                                    ? null : approvedParentfee18to36["ccof_frequency"].GetValue<int>(),
                            ["ccof_approvedparentfeefrequency3yk"] = (approvedParentfee3YK == null || approvedParentfee3YK[MonthLogicalName] == null ||
                                                                    approvedParentfee3YK[MonthLogicalName].GetValue<decimal>() == 0)
                                                                    ? null : approvedParentfee3YK["ccof_frequency"].GetValue<int>(),
                            ["ccof_approvedparentfeefrequencyoosck"] = (approvedParentfeeOOSCK == null || approvedParentfeeOOSCK[MonthLogicalName] == null ||
                                                                    approvedParentfeeOOSCK[MonthLogicalName].GetValue<decimal>() == 0)
                                                                    ? null : approvedParentfeeOOSCK["ccof_frequency"].GetValue<int>(),
                            ["ccof_approvedparentfeefrequencyooscg"] = (approvedParentfeeOOSCG == null || approvedParentfeeOOSCG[MonthLogicalName] == null ||
                                                                    approvedParentfeeOOSCG[MonthLogicalName].GetValue<decimal>() == 0)
                                                                    ? null : approvedParentfeeOOSCG["ccof_frequency"].GetValue<int>(),
                            ["ccof_approvedparentfeefrequencypre"] = (approvedParentfeePre == null || approvedParentfeePre[MonthLogicalName] == null ||
                                                                    approvedParentfeePre[MonthLogicalName].GetValue<decimal>() == 0)
                                                                    ? null : approvedParentfeePre["ccof_frequency"].GetValue<int>()
                        };
                        var dailyCCFRIRate = CalculateDailyCCFRIRate(approvedParentFee, (JsonObject)ccfriMax, (JsonObject)ccfriMin, feeFloorExempt, businessDay, providerType);
                        var EnrolmentReportToCreate = new JsonObject()
                        {
                            ["ccof_year"] = _processParams.InitialEnrolmentReport.Year,
                            ["ccof_month"] = _processParams.InitialEnrolmentReport.Month,
                            ["ccof_reportversion"] = 1,
                            ["ccof_feefloorexempt"] = feeFloorExempt,
                            ["ccof_providertype"] = providerType,
                            ["ccof_facility@odata.bind"] = $"/accounts(" + record + ")",
                            ["ccof_organization@odata.bind"] = $"/accounts(" + org["org.accountid"]?.GetValue<string>() + ")",
                            ["ccof_programyear@odata.bind"] = $"/ccof_program_years(" + _processParams.InitialEnrolmentReport.ProgramYearId + ")",
                            ["ccof_ccofbaserate@odata.bind"] = ccofBaseRate["ccof_rateid"]?.GetValue<string>() is string baseRateId ? $"/ccof_rates({baseRateId})" : null,
                            ["ccof_ccfriproviderpaymentrate@odata.bind"] = ccfriProviderPaymentRate["ccof_rateid"]?.GetValue<string>() is string providerRateId ? $"/ccof_rates({providerRateId})" : null,
                            ["ccof_ccfridailyratemax@odata.bind"] = ccfriMax["ccof_rateid"]?.GetValue<string>() is string CCFRIMaxRateId ? $"/ccof_rates({CCFRIMaxRateId})" : null,
                            ["ccof_ccfridailyratemin@odata.bind"] = ccfriMin["ccof_rateid"]?.GetValue<string>() is string CCFRIMinRateId ? $"/ccof_rates({CCFRIMinRateId})" : null,
                            ["ccof_reportextension"] = new JsonObject()
                            {
                                // Approved Parent Fee
                                ["ccof_approvedparentfee0to18"] = approvedParentFee["ccof_approvedparentfee18to36"]?.DeepClone(),
                                ["ccof_approvedparentfee18to36"] = approvedParentFee["ccof_approvedparentfee18to36"]?.DeepClone(),
                                ["ccof_approvedparentfee3yk"] = approvedParentFee["ccof_approvedparentfee3yk"]?.DeepClone(),
                                ["ccof_approvedparentfeeoosck"] = approvedParentFee["ccof_approvedparentfeeoosck"]?.DeepClone(),
                                ["ccof_approvedparentfeeooscg"] = approvedParentFee["ccof_approvedparentfeeooscg"]?.DeepClone(),
                                ["ccof_approvedparentfeepre"] = approvedParentFee["ccof_approvedparentfeepre"]?.DeepClone(),
                                ["ccof_approvedparentfeefrequency0to18"] = approvedParentFee["ccof_approvedparentfeefrequency0to18"]?.DeepClone(),
                                ["ccof_approvedparentfeefrequency18to36"] = approvedParentFee["ccof_approvedparentfeefrequency18to36"]?.DeepClone(),
                                ["ccof_approvedparentfeefrequency3yk"] = approvedParentFee["ccof_approvedparentfeefrequency3yk"]?.DeepClone(),
                                ["ccof_approvedparentfeefrequencyoosck"] = approvedParentFee["ccof_approvedparentfeefrequencyoosck"]?.DeepClone(),
                                ["ccof_approvedparentfeefrequencyooscg"] = approvedParentFee["ccof_approvedparentfeefrequencyooscg"]?.DeepClone(),
                                ["ccof_approvedparentfeefrequencypre"] = approvedParentFee["ccof_approvedparentfeefrequencypre"]?.DeepClone(),
                                // daily CCFRI Rate
                                ["ccof_dailyccfrirateless0to18"] = dailyCCFRIRate["ccof_dailyccfrirateless0to18"]?.DeepClone(),
                                ["ccof_dailyccfrirateover0to18"] = dailyCCFRIRate["ccof_dailyccfrirateover0to18"]?.DeepClone(),
                                ["ccof_dailyccfrirateless18to36"] = dailyCCFRIRate["ccof_dailyccfrirateless18to36"]?.DeepClone(),
                                ["ccof_dailyccfrirateover18to36"] = dailyCCFRIRate["ccof_dailyccfrirateover18to36"]?.DeepClone(),
                                ["ccof_dailyccfrirateless3yk"] = dailyCCFRIRate["ccof_dailyccfrirateless3yk"]?.DeepClone(),
                                ["ccof_dailyccfrirateover3yk"] = dailyCCFRIRate["ccof_dailyccfrirateover3yk"]?.DeepClone(),
                                ["ccof_dailccfriratelessoosck"] = dailyCCFRIRate["ccof_dailccfriratelessoosck"]?.DeepClone(),
                                ["ccof_dailyccfrirateoveroosck"] = dailyCCFRIRate["ccof_dailyccfrirateoveroosck"]?.DeepClone(),
                                ["ccof_dailyccfriratelessooscg"] = dailyCCFRIRate["ccof_dailyccfriratelessooscg"]?.DeepClone(),
                                ["ccof_dailyccfrirateoverooscg"] = dailyCCFRIRate["ccof_dailyccfrirateoverooscg"]?.DeepClone(),
                                ["ccof_dailyccfriratelesspre"] = dailyCCFRIRate["ccof_dailyccfriratelesspre"]?.DeepClone(),
                            },
                            ["ccof_dailyenrollment_monthlyenrollmentreport"] = new JsonArray(dailyEnrolment.Select(node => node.DeepClone()).ToArray())
                        };
                        createEnrolmentReportRequests.Add(new CreateRequest(entitySetName, EnrolmentReportToCreate));
                    }
                    var ERBatchResult = await _d365webapiservice.SendBatchMessageAsync(_appUserService.AZSystemAppUser, createEnrolmentReportRequests, null);
                    if (ERBatchResult.Errors.Any())
                    {
                        var errorInfos = ProcessResult.Failure(ProcessId, ERBatchResult.Errors, ERBatchResult.TotalProcessed, ERBatchResult.TotalRecords);

                        _logger.LogError(CustomLogEvent.Process, "Failed to Create Enrolment Report: {error}", JsonValue.Create(errorInfos)!.ToString());
                    }
                    _logger.LogDebug(CustomLogEvent.Process, "Create Batch process record index:{index}", i);
                }
                return ProcessResult.Completed(ProcessId).SimpleProcessResult;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                var returnObject = ProcessResult.Failure(ProcessId, new String[] { "Critical error", ex.StackTrace }, 0, 0).ODProcessResult;
                return returnObject;
            }
        }
    }
}