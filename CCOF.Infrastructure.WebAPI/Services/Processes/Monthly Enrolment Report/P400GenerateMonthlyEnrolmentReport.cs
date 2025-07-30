﻿//using ECC.Core.DataContext;
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
using System.Xml.Linq;
using Polly.Caching;

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
        public string RateRequestUritest
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
                return fetchXml;
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
                // fetchxml query is for reference only
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
                              <condition attribute="statecode" operator="eq" value="0" />
                              <condition attribute="statuscode" operator="eq" value="1" />
                              <condition attribute="ccof_type" operator="eq" value="1" />
                              <condition attribute="ccof_programyear" operator="eq" value="fdc2fce3-d1a2-ef11-8a6a-000d3af474a4" />
                            </filter>
                            <link-entity name="ccof_childcare_category" from="ccof_childcare_categoryid" to="ccof_childcarecategory" link-type="inner" alias="childcareCategory">
                              <attribute name="ccof_childcarecategorynumber" />
                              <attribute name="ccof_name" />
                              <order attribute="ccof_childcarecategorynumber" />
                            </link-entity>
                          </entity>
                        </fetch>
                        """;
                // var requestUri = $"ccof_parent_feeses?fetchXml=" + WebUtility.UrlEncode(fetchXml);
                var requestUri = $"ccof_parent_feeses?$select=ccof_apr,ccof_aug,ccof_availability,_ccof_childcarecategory_value,ccof_dec,_ccof_facility_value,ccof_feb,ccof_frequency,ccof_jan,ccof_jul,ccof_jun,ccof_mar,ccof_may,ccof_name,ccof_nov,ccof_oct,_ccof_programyear_value,ccof_sep,ccof_type,statecode,statuscode&$expand=ccof_ChildcareCategory($select=ccof_childcarecategorynumber,ccof_name)&$filter=(statecode eq 0 and statuscode eq 1 and ccof_type eq 1 and _ccof_programyear_value eq "+_processParams.InitialEnrolmentReport.ProgramYearId+") and (ccof_ChildcareCategory/ccof_childcare_categoryid ne null)";
                return requestUri.CleanCRLF();
            }
        }
        public string OrgRequestUri
        {
            get
            {
                // fetchxml query is for reference only
                // FacilityStatus not Closed, Not Null, not Cancelled
                var fetchXml = $$"""
                        <fetch>
                          <entity name="account">
                            <attribute name="name" />
                            <filter>
                              <condition attribute="ccof_accounttype" operator="eq" value="100000001" />
                              <condition attribute="parentaccountid" operator="not-null" />
                              <condition attribute="ccof_facilitystatus" operator="not-null" />
                              <condition attribute="ccof_facilitystatus" operator="ne" value="100000010" />
                              <condition attribute="ccof_facilitystatus" operator="ne" value="100000009" />
                            </filter>
                            <link-entity name="account" from="accountid" to="parentaccountid" link-type="inner" alias="org">
                              <attribute name="accountnumber" />
                              <attribute name="accountid" />
                            </link-entity>
                          </entity>
                        </fetch>
                        """;
                var requestUri = $"accounts?$select=name&$expand=parentaccountid($select=accountnumber,accountid)&$filter=(ccof_accounttype eq 100000001 and _parentaccountid_value ne null and ccof_facilitystatus ne null and ccof_facilitystatus ne 100000010 and ccof_facilitystatus ne 100000009) and (parentaccountid/accountid ne null)";
                // var requestUri = $"accounts?fetchXml=" + WebUtility.UrlEncode(fetchXml);
                return requestUri.CleanCRLF();

                // return requestUri;
            }
        }
        public string FacilityLicenceRequestUri
        {
            // fetchxml query is for reference only
            get
            {
                var fetchXml = $$"""
                        <fetch>
                          <entity name="ccof_facility_licenses">
                            <attribute name="ccof_facility" />
                            <attribute name="ccof_name" />
                            <attribute name="statecode" />
                            <attribute name="statuscode" />
                            <attribute name="ccof_licensecategory" />
                            <link-entity name="ccof_license_category" from="ccof_license_categoryid" to="ccof_licensecategory" link-type="inner" alias="licenceCategory">
                              <attribute name="ccof_categorynumber" />
                              <attribute name="ccof_name" />
                              <attribute name="ccof_providertype" />
                              <filter type="or">
                                <condition attribute="ccof_categorynumber" operator="eq" value="6" />
                                <condition attribute="ccof_categorynumber" operator="eq" value="5" />
                              </filter>
                            </link-entity>
                            <link-entity name="account" from="accountid" to="ccof_facility" link-type="inner" alias="facility">
                              <filter>
                                <condition attribute="ccof_accounttype" operator="eq" value="100000001" />
                              </filter>
                            </link-entity>
                          </entity>
                        </fetch>
                        """;
                // var requestUri = $"ccof_facility_licenseses?fetchXml=" + WebUtility.UrlEncode(fetchXml);
                var requestUri = $"ccof_facility_licenseses?$select=_ccof_facility_value,ccof_name,statecode,statuscode,_ccof_licensecategory_value&$expand=ccof_LicenseCategory($select=ccof_categorynumber,ccof_name,ccof_providertype),ccof_Facility($select=accountid)&$filter=(ccof_LicenseCategory/ccof_categorynumber eq 6 or ccof_LicenseCategory/ccof_categorynumber eq 5) and (ccof_Facility/ccof_accounttype eq 100000001)";
                return requestUri.CleanCRLF();
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
        private decimal? CalculateDailyParentFee(decimal? fee, int? frequency, int businessDay)
        {
            if (frequency == null)
                return null;
            if (frequency == 100000002)              // Monthly
                return fee;
            if (businessDay >= 20)
                return fee.HasValue ? fee.Value / 20 : null;
            else
                return fee.HasValue ? fee.Value / 19 : null;
        }
        decimal? CalculateRate(decimal? fee, bool isExempt, decimal deduction, bool isLess)
        {
            if (fee == null) return null;

            if (isLess)
            {
                return isExempt ? fee * 0.5m : (fee - deduction) * 0.5m;
            }
            else
            {
                return isExempt ? fee : fee - deduction;
            }
        }
        decimal? ApplyMinMaxCap(decimal? value, decimal? min, decimal? max)
        {
            if (value == null) return null;

            if (max != null && value > max)
                return max;
            if (min != null && value < min)
                return min;

            return value;
        }
        public JsonObject CalculateDailyCCFRIRate(JsonObject approvedParentFee, JsonObject ccfriMax, JsonObject ccfriMin, bool feeFloorExempt, int monthlyBusinessDay, int providerType)
        {
            var dailyParentFee0to18 = CalculateDailyParentFee(approvedParentFee["ccof_approvedparentfee0to18"]?.GetValue<decimal?>(),
                                       approvedParentFee["ccof_approvedparentfeefrequency0to18"]?.GetValue<int?>(), monthlyBusinessDay);
            var dailyParentFee18to36 = CalculateDailyParentFee(approvedParentFee["ccof_approvedparentfee18to36"]?.GetValue<decimal?>(),
                                        approvedParentFee["ccof_approvedparentfeefrequency18to36"]?.GetValue<int?>(),monthlyBusinessDay);
            var dailyParentFee3yk = CalculateDailyParentFee(approvedParentFee["ccof_approvedparentfee3yk"]?.GetValue<decimal?>(),
                approvedParentFee["ccof_approvedparentfeefrequency3yk"]?.GetValue<int?>(),monthlyBusinessDay);
            var dailyParentFeeoosck = CalculateDailyParentFee(approvedParentFee["ccof_approvedparentfeeoosck"]?.GetValue<decimal?>(),
                approvedParentFee["ccof_approvedparentfeefrequencyoosck"]?.GetValue<int?>(), monthlyBusinessDay);
            var dailyParentFeeooscg = CalculateDailyParentFee(approvedParentFee["ccof_approvedparentfeeooscg"]?.GetValue<decimal?>(),
                approvedParentFee["ccof_approvedparentfeefrequencyooscg"]?.GetValue<int?>(), monthlyBusinessDay);
            decimal? dailyParentFeepre = null;
            if (providerType == 100000000)   // Group
            {
                dailyParentFeepre = CalculateDailyParentFee(approvedParentFee["ccof_approvedparentfeepre"]?.GetValue<decimal?>(),
                    approvedParentFee["ccof_approvedparentfeefrequencypre"]?.GetValue<int?>(),monthlyBusinessDay      );
            }
            var dailyCCFRIRateMiddleStep = new Dictionary<string, decimal?>
            {
                ["ccof_dailyccfrirateless0to18"] = CalculateRate(dailyParentFee0to18, feeFloorExempt, 10, true),
                ["ccof_dailyccfrirateover0to18"] = CalculateRate(dailyParentFee0to18, feeFloorExempt, 10, false),

                ["ccof_dailyccfrirateless18to36"] = CalculateRate(dailyParentFee18to36, feeFloorExempt, 10, true),
                ["ccof_dailyccfrirateover18to36"] = CalculateRate(dailyParentFee18to36, feeFloorExempt, 10, false),

                ["ccof_dailyccfrirateless3yk"] = CalculateRate(dailyParentFee3yk, feeFloorExempt, 10, true),
                ["ccof_dailyccfrirateover3yk"] = CalculateRate(dailyParentFee3yk, feeFloorExempt, 10, false),

                ["ccof_dailccfriratelessoosck"] = CalculateRate(dailyParentFeeoosck, feeFloorExempt, 10, true),
                ["ccof_dailyccfrirateoveroosck"] = CalculateRate(dailyParentFeeoosck, feeFloorExempt, 10, false),

                ["ccof_dailyccfriratelessooscg"] = CalculateRate(dailyParentFeeooscg, feeFloorExempt, 10, true),
                ["ccof_dailyccfrirateoverooscg"] = CalculateRate(dailyParentFeeooscg, feeFloorExempt, 10, false),

                ["ccof_dailyccfriratelesspre"] = CalculateRate(dailyParentFeepre, feeFloorExempt, 7, false) 
            };
            var finaldailyCCFRIRate = new Dictionary<string, decimal?>
            {
                ["ccof_dailyccfrirateless0to18"] = ApplyMinMaxCap(dailyCCFRIRateMiddleStep["ccof_dailyccfrirateless0to18"], ccfriMin["ccof_less0to18"].GetValue<decimal>(), ccfriMax["ccof_less0to18"].GetValue<decimal>()),
                ["ccof_dailyccfrirateover0to18"] = ApplyMinMaxCap(dailyCCFRIRateMiddleStep["ccof_dailyccfrirateover0to18"], ccfriMin["ccof_over0to18"].GetValue<decimal>(), ccfriMax["ccof_over0to18"].GetValue<decimal>()),

                ["ccof_dailyccfrirateless18to36"] = ApplyMinMaxCap(dailyCCFRIRateMiddleStep["ccof_dailyccfrirateless18to36"], ccfriMin["ccof_less18to36"].GetValue<decimal>(), ccfriMax["ccof_less18to36"].GetValue<decimal>()),
                ["ccof_dailyccfrirateover18to36"] = ApplyMinMaxCap(dailyCCFRIRateMiddleStep["ccof_dailyccfrirateover18to36"], ccfriMin["ccof_over18to36"].GetValue<decimal>(), ccfriMax["ccof_over18to36"].GetValue<decimal>()),

                ["ccof_dailyccfrirateless3yk"] = ApplyMinMaxCap(dailyCCFRIRateMiddleStep["ccof_dailyccfrirateless3yk"], ccfriMin["ccof_less3yk"].GetValue<decimal>(), ccfriMax["ccof_less3yk"].GetValue<decimal>()),
                ["ccof_dailyccfrirateover3yk"] = ApplyMinMaxCap(dailyCCFRIRateMiddleStep["ccof_dailyccfrirateover3yk"], ccfriMin["ccof_over3yk"].GetValue<decimal>(), ccfriMax["ccof_over3yk"].GetValue<decimal>()),

                ["ccof_dailccfriratelessoosck"] = ApplyMinMaxCap(dailyCCFRIRateMiddleStep["ccof_dailccfriratelessoosck"], ccfriMin["ccof_lessoosck"].GetValue<decimal>(), ccfriMax["ccof_lessoosck"].GetValue<decimal>()),
                ["ccof_dailyccfrirateoveroosck"] = ApplyMinMaxCap(dailyCCFRIRateMiddleStep["ccof_dailyccfrirateoveroosck"], ccfriMin["ccof_overoosck"].GetValue<decimal>(), ccfriMax["ccof_overoosck"].GetValue<decimal>()),

                ["ccof_dailyccfriratelessooscg"] = ApplyMinMaxCap(dailyCCFRIRateMiddleStep["ccof_dailyccfriratelessooscg"], ccfriMin["ccof_lessooscg"].GetValue<decimal>(), ccfriMax["ccof_lessooscg"].GetValue<decimal>()),
                ["ccof_dailyccfrirateoverooscg"] = ApplyMinMaxCap(dailyCCFRIRateMiddleStep["ccof_dailyccfrirateoverooscg"], ccfriMin["ccof_overooscg"].GetValue<decimal>(), ccfriMax["ccof_overooscg"].GetValue<decimal>()),

                ["ccof_dailyccfriratelesspre"] = ApplyMinMaxCap(dailyCCFRIRateMiddleStep["ccof_dailyccfriratelesspre"], ccfriMin["ccof_lesspre"].GetValue<decimal>(), ccfriMax["ccof_lesspre"].GetValue<decimal>())
            };
             JsonObject dailyCCFRIRate = new JsonObject();

            foreach (var kvp in finaldailyCCFRIRate)
            {
                dailyCCFRIRate[kvp.Key] = kvp.Value is null ? null : JsonValue.Create(kvp.Value);
            }
            return dailyCCFRIRate; 
        }

        public async Task<JsonObject> RunProcessAsync(ID365AppUserService appUserService, ID365WebApiService d365WebApiService, ProcessParameter processParams)
        {
            var PSTZone = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");
            var pstTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, PSTZone);
            _logger.LogInformation(CustomLogEvent.Process, pstTime.ToString("yyyy-MM-dd HH:mm:ss")+" Begin to Initial ER process");
            try
            {
                var startTime = _timeProvider.GetTimestamp();
                _processParams = processParams;
                int businessDay = 0;
                var entitySetName = "ccof_monthlyenrollmentreports";
                // for Approved Parent fee
                var MonthLogicalNameTemp = JsonNode.Parse(monthLogicalNameString)?.AsArray() ?? throw new Exception("Invalid JSON");
                List<JsonNode> MonthLogicalNameArray = MonthLogicalNameTemp.Select(node => node!).ToList();
                string MonthLogicalName = MonthLogicalNameArray.FirstOrDefault(node => node["enrolmentMonth"].GetValue<int>() == _processParams.InitialEnrolmentReport.Month)["monthNameinApprovedParentFee"]?.GetValue<string>();
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
                //int batchSize = 1000;
                // int batchSize = 100;
                int batchSize = 50;

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
                            var accountNumber = org["parentaccountid"]?["accountnumber"]?.GetValue<string>();

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
                        //else
                        //{
                        //    throw new InvalidOperationException($"Organization with accountid '{record}' not found.");
                        //}
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
                        //var approvedParentfee0to18 = ApprovedParentFee.FirstOrDefault(node => node?["childcareCategory.ccof_childcarecategorynumber"]?.GetValue<int>() == 1 &&
                        //                        node?["_ccof_facility_value"]?.GetValue<string>() == record);  // for Fetchxml query
                        var approvedParentfee0to18 = ApprovedParentFee.FirstOrDefault(node => node?["ccof_ChildcareCategory"]?["ccof_childcarecategorynumber"]?.GetValue<int>() == 1 &&
                        node?["_ccof_facility_value"]?.GetValue<string>() == record);  // for odata query
                        var approvedParentfee18to36 = ApprovedParentFee.FirstOrDefault(node => node?["ccof_ChildcareCategory"]?["ccof_childcarecategorynumber"]?.GetValue<int>() == 2 &&
                                                node?["_ccof_facility_value"]?.GetValue<string>() == record);
                        var approvedParentfee3YK = ApprovedParentFee.FirstOrDefault(node => node?["ccof_ChildcareCategory"]?["ccof_childcarecategorynumber"]?.GetValue<int>() == 3 &&
                                                node?["_ccof_facility_value"]?.GetValue<string>() == record);
                        var approvedParentfeeOOSCK = ApprovedParentFee.FirstOrDefault(node => node?["ccof_ChildcareCategory"]?["ccof_childcarecategorynumber"]?.GetValue<int>() == 4 &&
                                                node?["_ccof_facility_value"]?.GetValue<string>() == record);
                        var approvedParentfeeOOSCG = ApprovedParentFee.FirstOrDefault(node => node?["ccof_ChildcareCategory"]?["ccof_childcarecategorynumber"]?.GetValue<int>() == 5 &&
                                                node?["_ccof_facility_value"]?.GetValue<string>() == record);
                        var approvedParentfeePre = ApprovedParentFee.FirstOrDefault(node => node?["ccof_ChildcareCategory"]?["ccof_childcarecategorynumber"]?.GetValue<int>() == 6 &&
                                                node?["_ccof_facility_value"]?.GetValue<string>() == record);
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
                            ["ccof_reporttype"] = 100000000, // 100000000 Baseline
                            ["ccof_feefloorexempt"] = feeFloorExempt,
                            ["ccof_providertype"] = providerType,
                            ["ccof_facility@odata.bind"] = $"/accounts(" + record + ")",
                            ["ccof_organization@odata.bind"] = (org==null)?null:$"/accounts(" + org["parentaccountid"]["accountid"]?.GetValue<string>() + ")",
                           // ["ccof_organization@odata.bind"] = (org == null) ? null : $"/accounts(" + org["org.accountid"]?.GetValue<string>() + ")",
                            ["ccof_programyear@odata.bind"] = $"/ccof_program_years(" + _processParams.InitialEnrolmentReport.ProgramYearId + ")",
                            ["ccof_ccofbaserate@odata.bind"] = ccofBaseRate?["ccof_rateid"]?.GetValue<string>() is string baseRateId ? $"/ccof_rates({baseRateId})" : null,
                            ["ccof_ccfriproviderpaymentrate@odata.bind"] = ccfriProviderPaymentRate?["ccof_rateid"]?.GetValue<string>() is string providerRateId ? $"/ccof_rates({providerRateId})" : null,
                            ["ccof_ccfridailyratemax@odata.bind"] = ccfriMax?["ccof_rateid"]?.GetValue<string>() is string CCFRIMaxRateId ? $"/ccof_rates({CCFRIMaxRateId})" : null,
                            ["ccof_ccfridailyratemin@odata.bind"] = ccfriMin?["ccof_rateid"]?.GetValue<string>() is string CCFRIMinRateId ? $"/ccof_rates({CCFRIMinRateId})" : null,
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
                    _logger.LogInformation(CustomLogEvent.Process, pstTime.ToString("yyyy-MM-dd HH:mm:ss")+" Create Batch process record index:{index}", i);
                    await Task.Delay(10000);  // deplay 10 seconds avoid api throtting.
                }
                var endtime = _timeProvider.GetTimestamp();
                var timediff = _timeProvider.GetElapsedTime(startTime, endtime).TotalSeconds;
                _logger.LogInformation(CustomLogEvent.Process, pstTime.ToString("yyyy-MM-dd HH:mm:ss")+" Total time:" + Math.Round(timediff, 2) + " seconds.\r\n");
                _logger.LogInformation(CustomLogEvent.Process, pstTime.ToString("yyyy-MM-dd HH:mm:ss")+ "End Create ER Batch process record");
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