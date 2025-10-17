using CCOF.Infrastructure.WebAPI.Extensions;
using CCOF.Infrastructure.WebAPI.Models;
using CCOF.Infrastructure.WebAPI.Services.D365WebApi;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System;
using System.Net;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Microsoft.IdentityModel.Tokens;

namespace CCOF.Infrastructure.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdjustmentERGenerationController : ControllerBase
    {
        private readonly ID365WebApiService _d365webapiservice;
        string programYearGuid = string.Empty;
        string facilityGuid = string.Empty;
        string ERGuid = string.Empty;
        int month = 0;
        string year = string.Empty;
        private readonly ILogger<AdjustmentERGenerationController> _logger;
        private readonly TimeProvider _timeProvider;
        public AdjustmentERGenerationController(ID365WebApiService d365webapiservice, ILogger<AdjustmentERGenerationController> logger, TimeProvider timeProvider)
        {
            _d365webapiservice = d365webapiservice ?? throw new ArgumentNullException(nameof(d365webapiservice));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
        }
        public string FacilityLicenceRequestUri
        {
            // fetchxml query is for reference only
            get
            {
                var fetchXml = $"""
                        <fetch>
                          <entity name="ccof_facility_licenses">
                            <attribute name="ccof_facility" />
                            <attribute name="ccof_name" />
                            <attribute name="statecode" />
                            <attribute name="statuscode" />
                            <attribute name="ccof_licensecategory" />
                            <filter>
                              <condition attribute="ccof_facility" operator="eq" value="{facilityGuid}" />
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
        public string CCFRIFacilityRequestUri
        {  
            get
            {
                var fetchXml = $"""
                    <fetch>
                      <entity name="ccof_adjudication_ccfri_facility">
                        <attribute name="ccof_ccfripaymenteligibilitystartdate" />
                        <attribute name="ccof_name" />
                        <attribute name="ccof_facility" />
                        <filter>
                          <condition attribute="ccof_facility" operator="eq" value="{facilityGuid}" />
                          <condition attribute="ccof_programyear" operator="eq" value="{programYearGuid}" />
                        </filter>
                      </entity>
                    </fetch>
                    """;
                var requestUri = $"ccof_adjudication_ccfri_facilities?fetchXml=" + WebUtility.UrlEncode(fetchXml);
                return requestUri.CleanCRLF();
            }
        }
        public string ApprovedClosureDayRequestUri
        {  // 100000001 Complete_Approved
            get
            {
                var fetchXml = $"""
                    <fetch>
                      <entity name="ccof_application_ccfri_closure">
                        <attribute name="ccof_program_year" />
                        <attribute name="ccof_age_affected_groups" />
                        <attribute name="ccof_closure_status" />
                        <attribute name="ccof_closure_type" />
                        <attribute name="ccof_comment" />
                        <attribute name="ccof_emergency_closure_type" />
                        <attribute name="ccof_enddate" />
                        <attribute name="ccof_is_full_closure" />
                        <attribute name="ccof_name" />
                        <attribute name="ccof_organizationfacility" />
                        <attribute name="ccof_startdate" />
                        <attribute name="ccof_totaldays" />
                        <attribute name="ccof_totalworkdays" />
                        <attribute name="statecode" />
                        <attribute name="statuscode" />
                        <attribute name="ccof_facilityinfo" />
                        <attribute name="ccof_paidclosure" />
                        <attribute name="ccof_pastercorrected" />
                        <attribute name="ccof_payment_eligibility" />
                        <filter>
                          <condition attribute="ccof_closure_status" operator="eq" value="100000001" />  
                          <condition attribute="ccof_program_year" operator="eq" value="{programYearGuid}" />
                          <condition attribute="statecode" operator="eq" value="0" />
                        </filter>
                      </entity>
                    </fetch>
                    """;
                var requestUri = $"ccof_application_ccfri_closures?fetchXml=" + WebUtility.UrlEncode(fetchXml);
                return requestUri.CleanCRLF();
            }
        }
        public string FeeFloorExemptRequestUri
        {
            get
            {
                var fetchXml = $"""
                    <fetch>
                      <entity name="ccof_feefloorexempt">
                        <attribute name="ccof_facility" />
                        <attribute name="ccof_months" />
                        <attribute name="ccof_name" />
                        <attribute name="ccof_programyear" />
                        <filter>
                          <condition attribute="ccof_programyear" operator="eq" value="{programYearGuid}" />
                          <condition attribute="ccof_facility" operator="eq" value="{facilityGuid}" />
                          <condition attribute="statecode" operator="eq" value="0" />
                        </filter>
                      </entity>
                    </fetch>
                    """;
                var requestUri = $"ccof_feefloorexempts?fetchXml=" + WebUtility.UrlEncode(fetchXml);
                return requestUri.CleanCRLF();
            }
        }
        public string MonthlyERRequestUri
        {
            get
            {   // fetchxml for reference.
                var fetchXml = $"""
                    <fetch>
                      <entity name="ccof_monthlyenrollmentreport">
                     <attribute name="ccof_is_full_month_closure" />
                        <filter type="and">
                          <condition attribute="ccof_monthlyenrollmentreportid" operator="eq" value="57e08f64-8569-f011-bec2-6045bdf991c2" />
                        </filter>
                        <link-entity name="ccof_monthlyenrolmentreportextension" from="ccof_monthlyenrolmentreportextensionid" to="ccof_reportextension" />
                        <link-entity name="ccof_dailyenrollment" from="ccof_monthlyenrollmentreport" to="ccof_monthlyenrollmentreportid" link-type="inner" alias="dailyEnrolment" />
                        <link-entity name="ccof_rate" from="ccof_rateid" to="ccof_ccfridailyratemax" link-type="inner" alias="ccfrimax" />
                        <link-entity name="ccof_rate" from="ccof_rateid" to="ccof_ccfridailyratemin" link-type="inner" alias="ccfrimin" />
                      </entity>
                    </fetch>
                    """;
                var requestUri = $"ccof_monthlyenrollmentreports(" + ERGuid + ")?$expand=ccof_reportextension,ccof_ccfridailyratemax,ccof_ccfridailyratemin,ccof_dailyenrollment_monthlyenrollmentreport";
                return requestUri.CleanCRLF();

            }
        }
        public string ERVersionNumRequestUri
        {
            get
            {
                var fetchXml = $$"""
                        <fetch top="1">
                          <entity name="ccof_monthlyenrollmentreport">
                            <attribute name="ccof_facility" />
                            <attribute name="ccof_name" />
                            <attribute name="ccof_reportversion" />
                            <filter type="and">
                              <condition attribute="ccof_facility" operator="eq" value="{{facilityGuid}}" />
                              <condition attribute="ccof_month" operator="eq" value="{{month}}" />
                              <condition attribute="ccof_year" operator="eq" value="{{year}}" />
                            </filter>
                            <order attribute="ccof_reportversion" descending="true" />
                          </entity>
                        </fetch>
                        """;
                var requestUri = $"ccof_monthlyenrollmentreports?fetchXml=" + WebUtility.UrlEncode(fetchXml);
                return requestUri;
            }
        }
        public string ApprovedParentFeeRequestUri
        {
            get
            {
                // Fetchxml is for reference
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
                              <condition attribute="ccof_programyear" operator="eq" value="{{programYearGuid}}" />
                              <condition attribute="ccof_facility" operator="eq" value="{{facilityGuid}}" />
                            </filter>
                            <link-entity name="ccof_childcare_category" from="ccof_childcare_categoryid" to="ccof_childcarecategory" link-type="inner" alias="childcareCategory">
                              <attribute name="ccof_childcarecategorynumber" />
                              <attribute name="ccof_name" />
                              <order attribute="ccof_childcarecategorynumber" />
                            </link-entity>
                          </entity>
                        </fetch>
                        """;
                //  var requestUri = $"ccof_parent_feeses?fetchXml=" + WebUtility.UrlEncode(fetchXml);
                var requestUri = $"ccof_parent_feeses?$select=ccof_apr,ccof_aug,ccof_availability,_ccof_childcarecategory_value,ccof_dec,_ccof_facility_value,ccof_feb,ccof_frequency,ccof_jan,ccof_jul," +
                    $"ccof_jun,ccof_mar,ccof_may,ccof_name,ccof_nov,ccof_oct,_ccof_programyear_value,ccof_sep,ccof_type,statecode,statuscode&$expand=ccof_ChildcareCategory($select=ccof_childcarecategorynumber,ccof_name)" +
                    $"&$filter=(statecode eq 0 and statuscode eq 1 and _ccof_programyear_value eq " + programYearGuid + " and _ccof_facility_value eq "
                    + facilityGuid + ") and (ccof_ChildcareCategory/ccof_childcare_categoryid ne null)";
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
        private decimal? CalculateDailyParentFee(decimal? fee, int? frequency, int businessDay)
        {
            if (fee == null)
                return null;
            if (frequency == null)
                return null;
            if (frequency == 100000002)              // Daily
                return fee;
            if (businessDay >= 20)
                return fee.HasValue ? fee.Value / 20 : null;
            else
                return fee.HasValue ? fee.Value / 19 : null;
        }
        private decimal? CalculateRate(decimal? fee, bool isExempt, decimal deduction, bool isLess)
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
        private decimal? ApplyMinMaxCap(decimal? value, decimal? min, decimal? max)
        {
            if (value == null) return null;

            if (max != null && value > max)
                return max;
            if (min != null && value < min)
                return min;

            return value;
        }
        private JsonObject CalculateDailyCCFRIRate(JsonObject approvedParentFee, JsonObject ccfriMax, JsonObject ccfriMin, bool feeFloorExempt, int monthlyBusinessDay, int providerType)
        {
            var dailyParentFee0to18 = CalculateDailyParentFee(approvedParentFee["ccof_approvedparentfee0to18"]?.GetValue<decimal?>(),
                                       approvedParentFee["ccof_approvedparentfeefrequency0to18"]?.GetValue<int?>(), monthlyBusinessDay);
            var dailyParentFee18to36 = CalculateDailyParentFee(approvedParentFee["ccof_approvedparentfee18to36"]?.GetValue<decimal?>(),
                                        approvedParentFee["ccof_approvedparentfeefrequency18to36"]?.GetValue<int?>(), monthlyBusinessDay);
            var dailyParentFee3yk = CalculateDailyParentFee(approvedParentFee["ccof_approvedparentfee3yk"]?.GetValue<decimal?>(),
                approvedParentFee["ccof_approvedparentfeefrequency3yk"]?.GetValue<int?>(), monthlyBusinessDay);
            var dailyParentFeeoosck = CalculateDailyParentFee(approvedParentFee["ccof_approvedparentfeeoosck"]?.GetValue<decimal?>(),
                approvedParentFee["ccof_approvedparentfeefrequencyoosck"]?.GetValue<int?>(), monthlyBusinessDay);
            var dailyParentFeeooscg = CalculateDailyParentFee(approvedParentFee["ccof_approvedparentfeeooscg"]?.GetValue<decimal?>(),
                approvedParentFee["ccof_approvedparentfeefrequencyooscg"]?.GetValue<int?>(), monthlyBusinessDay);
            decimal? dailyParentFeepre = null;
            if (providerType == 100000000)   // Group
            {
                dailyParentFeepre = CalculateDailyParentFee(approvedParentFee["ccof_approvedparentfeepre"]?.GetValue<decimal?>(),
                    approvedParentFee["ccof_approvedparentfeefrequencypre"]?.GetValue<int?>(), monthlyBusinessDay);
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
        // Generate Adjustment Enrolment Report
        [HttpPost]
        [ActionName("GenerateAdjustmentER")]
       //  public ActionResult<string> GenerateAdjusementER([FromBody] dynamic value)
        public ActionResult<string> GenerateAdjusementER([FromBody] AdjustmentERRequest request) 
        {
            var PSTZone = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");
            var pstTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, PSTZone);
            var startTime = _timeProvider.GetTimestamp();
            ERGuid = request.ERGuid;
            string targetEntityLogicalName = request.targetEntityLogicalName;
            string targetEntitySetName = request.targetEntitySetName;
            string targetRecordGuid = request.targetRecordGuid;
            string lookupFieldSchemaName = "ccof_adjustment_created_by";  
            _logger.LogInformation(pstTime.ToString("yyyy-MM-dd HH:mm:ss") + " Endpoint: GenerateAdjusementER Starting GenerateAdjusementER for Enrolment Report ID: {ERGuid}", ERGuid.Replace("\r", "").Replace("\n", ""));
            HttpResponseMessage response = null;
            // get Previous ER
            response = _d365webapiservice.SendRetrieveRequestAsync(MonthlyERRequestUri, true);
            JObject PreviousER = JObject.Parse(response.Content.ReadAsStringAsync().Result.ToString());
            programYearGuid = PreviousER["_ccof_programyear_value"]?.ToString();
            facilityGuid = PreviousER["_ccof_facility_value"]?.ToString();
            month = (int)PreviousER["ccof_month"];
            int fiscalMonth = ((month + 8) % 12) + 1;
            year = PreviousER["ccof_year"]?.ToString().Trim();
            JsonNode? ccfriMax = JsonObject.Parse(PreviousER["ccof_ccfridailyratemax"].ToString());
            JsonNode? ccfriMin = JsonObject.Parse(PreviousER["ccof_ccfridailyratemin"].ToString());
            response = _d365webapiservice.SendRetrieveRequestAsync(FeeFloorExemptRequestUri, true);
            JObject FeeFloorExemptObject = JObject.Parse(response.Content.ReadAsStringAsync().Result.ToString());
            FeeFloorExemptObject = (JObject)FeeFloorExemptObject["value"]?.FirstOrDefault();
            bool feeFloorExempt = false;
            feeFloorExempt = FeeFloorExemptObject != null && FeeFloorExemptObject["ccof_months"] != null && FeeFloorExemptObject["ccof_months"]
                          .ToString()
                          .Split(',')
                          .Select(v => int.Parse(v.Trim()))
                          .Contains(fiscalMonth);
            int providerType = PreviousER["ccof_providertype"]?.Value<int?>() ?? 100000001; // Family
            int businessDay = ccfriMax?["ccof_businessday"]?.GetValue<int>() ?? 20;
            // get Monthlogicalname in Approved Parent Fee
            var MonthLogicalNameTemp = JsonNode.Parse(monthLogicalNameString)?.AsArray() ?? throw new Exception("Invalid JSON");
            List<JsonNode> MonthLogicalNameArray = MonthLogicalNameTemp.Select(node => node!).ToList();
            string MonthLogicalName = MonthLogicalNameArray.FirstOrDefault(node => node["enrolmentMonth"].GetValue<int>() == PreviousER["ccof_month"]?.Value<int?>())["monthNameinApprovedParentFee"]?.GetValue<string>();
            // get Approved Parent Fee
            response = _d365webapiservice.SendRetrieveRequestAsync($"{ApprovedParentFeeRequestUri}", true);
            JObject ApprovedParentFeejsonObject = JObject.Parse(response.Content.ReadAsStringAsync().Result);
            JArray? valueArray = (JArray)ApprovedParentFeejsonObject["value"];
            List<JsonNode> allApprovedParentFees = valueArray.Select(j => JsonNode.Parse(j.ToString())).ToList();
            // calculate Approved Parent fees
            JsonNode? approvedParentfee0to18 = null;
            JsonNode? approvedParentfee18to36 = null;
            JsonNode? approvedParentfee3YK = null;
            JsonNode? approvedParentfeeOOSCK = null;
            JsonNode? approvedParentfeeOOSCG = null;
            JsonNode? approvedParentfeePre = null;
            if (allApprovedParentFees != null && allApprovedParentFees.Count > 0)
            {
                var firstRecord = allApprovedParentFees[0].AsObject();
                int? type = firstRecord["ccof_type"]?.GetValue<int>();
                if (type == 1)
                {
                    response = _d365webapiservice.SendRetrieveRequestAsync(CCFRIFacilityRequestUri);
                    JObject CCFRIFacilityJsonObject = JObject.Parse(response.Content.ReadAsStringAsync().Result);
                    JArray? CCFRIFacilityArray = (JArray)CCFRIFacilityJsonObject["value"];
                    List<JsonNode> allCCFRIFacility = CCFRIFacilityArray.Select(j => JsonNode.Parse(j.ToString())).ToList();
                    if (allCCFRIFacility!=null && allCCFRIFacility.Count>0)
                    {
                        var CCFRIFacility = allCCFRIFacility[0].AsObject();
                        _logger.LogInformation(pstTime.ToString("yyyy-MM-dd HH:mm:ss") + " Endpoint: GenerateAdjusementER  This is Fully Approval Parent fees with CCFRIFacilityGuid: " + CCFRIFacility["ccof_adjudication_ccfri_facilityid"]);
                        DateTime? eligibilityStartDate = CCFRIFacility["ccof_ccfripaymenteligibilitystartdate"]?.GetValue<DateTime?>();
                        var dateToCompare = new DateTime(int.Parse(year), month, 1);
                        if (eligibilityStartDate != null && dateToCompare.Date >= eligibilityStartDate.Value.Date)
                        {
                            approvedParentfee0to18 = allApprovedParentFees.FirstOrDefault(node => node?["ccof_ChildcareCategory"]?["ccof_childcarecategorynumber"]?.GetValue<int>() == 1);
                            approvedParentfee18to36 = allApprovedParentFees.FirstOrDefault(node => node?["ccof_ChildcareCategory"]?["ccof_childcarecategorynumber"]?.GetValue<int>() == 2);
                            approvedParentfee3YK = allApprovedParentFees.FirstOrDefault(node => node?["ccof_ChildcareCategory"]?["ccof_childcarecategorynumber"]?.GetValue<int>() == 3);
                            approvedParentfeeOOSCK = allApprovedParentFees.FirstOrDefault(node => node?["ccof_ChildcareCategory"]?["ccof_childcarecategorynumber"]?.GetValue<int>() == 4);
                            approvedParentfeeOOSCG = allApprovedParentFees.FirstOrDefault(node => node?["ccof_ChildcareCategory"]?["ccof_childcarecategorynumber"]?.GetValue<int>() == 5);
                            approvedParentfeePre = allApprovedParentFees.FirstOrDefault(node => node?["ccof_ChildcareCategory"]?["ccof_childcarecategorynumber"]?.GetValue<int>() == 6);
                        }
                        // else — before eligibility start date, variables stay null
                    }
                    // else — facility record not found, skip
                }
                else
                {
                    _logger.LogInformation(pstTime.ToString("yyyy-MM-dd HH:mm:ss") + " Endpoint: GenerateAdjusementER  This is temp Approval Parent fees:");

                }
                // else — not fully approved, skip
            }
            var approvedParentFeesForMonth = new JsonObject()
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
                                                        ? null : approvedParentfeeOOSCK[MonthLogicalName].GetValue<decimal>(),
                ["ccof_approvedparentfeeooscg"] = (approvedParentfeeOOSCG == null || approvedParentfeeOOSCG[MonthLogicalName] == null ||
                                                        approvedParentfeeOOSCG[MonthLogicalName].GetValue<decimal>() == 0)
                                                        ? null : approvedParentfeeOOSCG[MonthLogicalName].GetValue<decimal>(),
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
            _logger.LogInformation(pstTime.ToString("yyyy-MM-dd HH:mm:ss") + " Endpoint: GenerateAdjusementER approvedParentFee json string: " + approvedParentFeesForMonth.ToJsonString());
            string? providerPaymentRateBind = null;
            bool allFeesEmptyOrZero =
                (approvedParentFeesForMonth["ccof_approvedparentfee0to18"] == null || approvedParentFeesForMonth["ccof_approvedparentfee0to18"].GetValue<decimal>() == 0) &&
                (approvedParentFeesForMonth["ccof_approvedparentfee18to36"] == null || approvedParentFeesForMonth["ccof_approvedparentfee18to36"].GetValue<decimal>() == 0) &&
                (approvedParentFeesForMonth["ccof_approvedparentfee3yk"] == null || approvedParentFeesForMonth["ccof_approvedparentfee3yk"].GetValue<decimal>() == 0) &&
                (approvedParentFeesForMonth["ccof_approvedparentfeeoosck"] == null || approvedParentFeesForMonth["ccof_approvedparentfeeoosck"].GetValue<decimal>() == 0) &&
                (approvedParentFeesForMonth["ccof_approvedparentfeeooscg"] == null || approvedParentFeesForMonth["ccof_approvedparentfeeooscg"].GetValue<decimal>() == 0) &&
                (approvedParentFeesForMonth["ccof_approvedparentfeepre"] == null || approvedParentFeesForMonth["ccof_approvedparentfeepre"].GetValue<decimal>() == 0);
            if (!allFeesEmptyOrZero)
            {
                response = _d365webapiservice.SendRetrieveRequestAsync(RateRequestUri);
                JObject rateJsonObject = JObject.Parse(response.Content.ReadAsStringAsync().Result);
                JArray? rateArray = (JArray)rateJsonObject["value"];
                List<JsonNode> rate = rateArray.Select(j => JsonNode.Parse(j.ToString())).ToList();
                response = _d365webapiservice.SendRetrieveRequestAsync(FacilityLicenceRequestUri);
                JObject FacilityLicenseJsonObject = JObject.Parse(response.Content.ReadAsStringAsync().Result);
                JArray? FacilityLicenseArray = (JArray)FacilityLicenseJsonObject["value"];
                List<JsonNode> facilityLicence = FacilityLicenseArray.Select(j => JsonNode.Parse(j.ToString())).ToList();
                bool IHMALicenceExist = facilityLicence != null && facilityLicence.Count > 0;
                var ccfriProviderPaymentRate = rate.FirstOrDefault(node => node?["ccof_providertype"]?.GetValue<int>() == providerType &&
                                       node?["ccof_ratetype"]?.GetValue<int>() == (int)(IHMALicenceExist ? 100000003 : 100000002)); // 100000003 IHMA Provider Payment Rate;100000002 CCFRI Provider Payment;
                if (ccfriProviderPaymentRate != null && ccfriProviderPaymentRate["ccof_rateid"] != null)
                {
                    providerPaymentRateBind = $"/ccof_rates({ccfriProviderPaymentRate["ccof_rateid"]?.GetValue<string>()})";
                }
            }
            else
            {
                providerPaymentRateBind = null;
            }
            // recalculate Daily CCFRI Rate
            var dailyCCFRIRate = CalculateDailyCCFRIRate(approvedParentFeesForMonth, (JsonObject)ccfriMax, (JsonObject)ccfriMin, feeFloorExempt, businessDay, providerType);
            // get largest version number
            response = _d365webapiservice.SendRetrieveRequestAsync(ERVersionNumRequestUri, true);
            var ERforVersionNumber = JObject.Parse(response.Content.ReadAsStringAsync().Result.ToString());
            int? reportVersion = ERforVersionNumber["value"]?.FirstOrDefault()?["ccof_reportversion"]?.Value<int?>();
            var dailyEnrollmentArray = PreviousER["ccof_dailyenrollment_monthlyenrollmentreport"] as JArray;
            var dailyEnrollmentSelected = new JsonArray();
            response = _d365webapiservice.SendRetrieveRequestAsync(ApprovedClosureDayRequestUri, true);
            var approvedClosureDays = JObject.Parse(response.Content.ReadAsStringAsync().Result.ToString());
            JArray? approvedClosureDaysArray = approvedClosureDays["value"] as JArray;
            _logger.LogInformation(pstTime.ToString("yyyy-MM-dd HH:mm:ss") + " Endpoint: GenerateAdjusementER Raw Approved Closures JSON: " + approvedClosureDaysArray?.ToString() ?? "[]");

            if (dailyEnrollmentArray != null)
            {
                foreach (var item in dailyEnrollmentArray)
                {
                    var itemObj = item as JObject;
                    if (itemObj == null) continue;
                    var selectedObject = new JsonObject();
                    // check Approved Closure Days and Type
                    int dayOfMonth = itemObj["ccof_day"].Value<int>();
                    selectedObject["ccof_day"] = JsonValue.Create(dayOfMonth);
                    DateTime currentDayEnrollmentDate = new DateTime(int.Parse(year), month, dayOfMonth);
                    // Check if this day falls within any approved closure period
                    int? closurePaymentEligibility = null;
                    if (approvedClosureDaysArray != null) 
                    {
                        foreach (JObject closure in approvedClosureDaysArray) 
                        {
                            if (closure["ccof_startdate"] != null && closure["ccof_enddate"] != null && closure["ccof_payment_eligibility"] != null)
                            {
                                DateTime startDate = closure["ccof_startdate"].Value<DateTime>().Date;
                                DateTime endDate = closure["ccof_enddate"].Value<DateTime>().Date;
                                int paymentEligibility = closure["ccof_payment_eligibility"].Value<int>();
                                if (currentDayEnrollmentDate >= startDate && currentDayEnrollmentDate <= endDate)
                                {
                                    closurePaymentEligibility = paymentEligibility;
                                   //  _logger.LogInformation($"Daily enrollment day {currentDayEnrollmentDate.ToShortDateString()} is within closure {startDate.ToShortDateString()} - {endDate.ToShortDateString()}. Setting payment eligibility to {paymentEligibility}");
                                    break; 
                                }
                            }
                        }
                    }
                    // Set ccof_paymenteligibility if a closure match was found
                    if (closurePaymentEligibility.HasValue)
                    {
                        selectedObject["ccof_paymenteligibility"] = JsonValue.Create(closurePaymentEligibility.Value);
                    }
                    if (itemObj["ccof_day"] != null) selectedObject["ccof_day"] = JsonValue.Create(itemObj["ccof_day"].Value<int?>());
                    if (itemObj["ccof_daytype"] != null) selectedObject["ccof_daytype"] = JsonValue.Create(itemObj["ccof_daytype"].Value<int?>());
                    if (itemObj["ccof_less0to18"] != null) selectedObject["ccof_less0to18"] = JsonValue.Create(itemObj["ccof_less0to18"].Value<decimal?>());
                    if (itemObj["ccof_over0to18"] != null) selectedObject["ccof_over0to18"] = JsonValue.Create(itemObj["ccof_over0to18"].Value<decimal?>());
                    if (itemObj["ccof_less18to36"] != null) selectedObject["ccof_less18to36"] = JsonValue.Create(itemObj["ccof_less18to36"].Value<decimal?>());
                    if (itemObj["ccof_over18to36"] != null) selectedObject["ccof_over18to36"] = JsonValue.Create(itemObj["ccof_over18to36"].Value<decimal?>());
                    if (itemObj["ccof_less3yk"] != null) selectedObject["ccof_less3yk"] = JsonValue.Create(itemObj["ccof_less3yk"].Value<decimal?>());
                    if (itemObj["ccof_over3yk"] != null) selectedObject["ccof_over3yk"] = JsonValue.Create(itemObj["ccof_over3yk"].Value<decimal?>());
                    if (itemObj["ccof_lessooscg"] != null) selectedObject["ccof_lessooscg"] = JsonValue.Create(itemObj["ccof_lessooscg"].Value<decimal?>());
                    if (itemObj["ccof_overooscg"] != null) selectedObject["ccof_overooscg"] = JsonValue.Create(itemObj["ccof_overooscg"].Value<decimal?>());
                    if (itemObj["ccof_lessoosck"] != null) selectedObject["ccof_lessoosck"] = JsonValue.Create(itemObj["ccof_lessoosck"].Value<decimal?>());
                    if (itemObj["ccof_overoosck"] != null) selectedObject["ccof_overoosck"] = JsonValue.Create(itemObj["ccof_overoosck"].Value<decimal?>());
                    if (itemObj["ccof_lesspre"] != null) selectedObject["ccof_lesspre"] = JsonValue.Create(itemObj["ccof_lesspre"].Value<decimal?>());
                    dailyEnrollmentSelected.Add(selectedObject);
                }
            }
            var EnrolmentReportToCreate = new JsonObject()
            {
                ["ccof_year"] = PreviousER["ccof_year"]?.ToString(),
                ["ccof_month"] = PreviousER["ccof_month"]?.Value<int?>(),
                ["ccof_is_full_month_closure"] = PreviousER["ccof_is_full_month_closure"]?.Value<bool?>(),
                ["ccof_reporttype"] = 100000001, // Adjustment
                ["ccof_reportversion"] = reportVersion + 1,
                ["ccof_feefloorexempt"] = feeFloorExempt,
                ["ccof_providertype"] = PreviousER["ccof_providertype"]?.Value<int?>(),
                ["ccof_originalenrollmentreport@odata.bind"] = $"/ccof_monthlyenrollmentreports(" + ((PreviousER["_ccof_originalenrollmentreport_value"]?.ToString() == "") ? PreviousER["ccof_monthlyenrollmentreportid"]?.ToString() :
                                                                                                    PreviousER["_ccof_originalenrollmentreport_value"].ToString()) + ")",
                ["ccof_prevenrollmentreport@odata.bind"] = $"/ccof_monthlyenrollmentreports(" + PreviousER["ccof_monthlyenrollmentreportid"]?.ToString() + ")",
                ["ccof_facility@odata.bind"] = $"/accounts(" + PreviousER["_ccof_facility_value"]?.ToString() + ")",
                ["ccof_organization@odata.bind"] = $"/accounts(" + PreviousER["_ccof_organization_value"]?.ToString() + ")",
                ["ccof_programyear@odata.bind"] = $"/ccof_program_years(" + PreviousER["_ccof_programyear_value"].ToString() + ")",
                ["ccof_ccofbaserate@odata.bind"] = PreviousER["_ccof_ccofbaserate_value"] == null ? null : $"/ccof_rates({PreviousER["_ccof_ccofbaserate_value"].ToString()})",
                ["ccof_ccfriproviderpaymentrate@odata.bind"] = providerPaymentRateBind,
                ["ccof_ccfridailyratemax@odata.bind"] = PreviousER["_ccof_ccfridailyratemax_value"]== null ? null:$"/ccof_rates(" + PreviousER["_ccof_ccfridailyratemax_value"].ToString() + ")",
                ["ccof_ccfridailyratemin@odata.bind"] = PreviousER["_ccof_ccfridailyratemin_value"]== null ? null:$"/ccof_rates(" + PreviousER["_ccof_ccfridailyratemin_value"].ToString() + ")",
                [$"{lookupFieldSchemaName}_{targetEntityLogicalName}@odata.bind"] = (string.IsNullOrEmpty(targetEntityLogicalName) || string.IsNullOrEmpty(targetEntitySetName) || string.IsNullOrEmpty(targetRecordGuid)) ? null : $"/{targetEntitySetName}({targetRecordGuid})",
                #region main fields need to copied to Adjustment ER
                // Total Enrolled
                ["ccof_totalenrolled0to18"] = PreviousER["ccof_totalenrolled0to18"]?.Value<int?>(),
                ["ccof_totalenrolled18to36"] = PreviousER["ccof_totalenrolled18to36"]?.Value<int?>(),
                ["ccof_totalenrolled3yk"] = PreviousER["ccof_totalenrolled3yk"]?.Value<int?>(),
                ["ccof_totalenrolledoosck"] = PreviousER["ccof_totalenrolledoosck"]?.Value<int?>(),
                ["ccof_totalenrolledooscg"] = PreviousER["ccof_totalenrolledooscg"]?.Value<int?>(),
                ["ccof_totalenrolledpre"] = PreviousER["ccof_totalenrolledpre"]?.Value<int?>(),
                // Current Total
                ["ccof_currenttotalless0to18"] = PreviousER["ccof_currenttotalless0to18"]?.Value<int?>(),
                ["ccof_currenttotalover0to18"] = PreviousER["ccof_currenttotalover0to18"]?.Value<int?>(),
                ["ccof_currenttotalless18to36"] = PreviousER["ccof_currenttotalless18to36"]?.Value<int?>(),
                ["ccof_currenttotalover18to36"] = PreviousER["ccof_currenttotalover18to36"]?.Value<int?>(),
                ["ccof_currenttotalless3yk"] = PreviousER["ccof_currenttotalless3yk"]?.Value<int?>(),
                ["ccof_currenttotalover3yk"] = PreviousER["ccof_currenttotalover3yk"]?.Value<int?>(),
                ["ccof_currenttotallessoosck"] = PreviousER["ccof_currenttotallessoosck"]?.Value<int?>(),
                ["ccof_currenttotaloveroosck"] = PreviousER["ccof_currenttotaloveroosck"]?.Value<int?>(),
                ["ccof_currenttotallessooscg"] = PreviousER["ccof_currenttotallessooscg"]?.Value<int?>(),
                ["ccof_currenttotaloverooscg"] = PreviousER["ccof_currenttotaloverooscg"]?.Value<int?>(),
                ["ccof_currenttotallesspre"] = PreviousER["ccof_currenttotallesspre"]?.Value<int?>(),
                // Current $
                ["ccof_ccofbaseamountless0to18"] = PreviousER["ccof_ccofbaseamountless0to18"]?.Value<decimal?>(),
                ["ccof_ccofbaseamountover0to18"] = PreviousER["ccof_ccofbaseamountover0to18"]?.Value<decimal?>(),
                ["ccof_ccofbaseamountless18to36"] = PreviousER["ccof_ccofbaseamountless18to36"]?.Value<decimal?>(),
                ["ccof_ccofbaseamountover18to36"] = PreviousER["ccof_ccofbaseamountover18to36"]?.Value<decimal?>(),
                ["ccof_ccofbaseamountless3yk"] = PreviousER["ccof_ccofbaseamountless3yk"]?.Value<decimal?>(),
                ["ccof_ccofbaseamountover3yk"] = PreviousER["ccof_ccofbaseamountover3yk"]?.Value<decimal?>(),
                ["ccof_ccofbaseamountlessoosck"] = PreviousER["ccof_ccofbaseamountlessoosck"]?.Value<decimal?>(),
                ["ccof_ccofbaseamountoveroosck"] = PreviousER["ccof_ccofbaseamountoveroosck"]?.Value<decimal?>(),
                ["ccof_ccofbaseamountlessooscg"] = PreviousER["ccof_ccofbaseamountlessooscg"]?.Value<decimal?>(),
                ["ccof_ccofbaseamountoverooscg"] = PreviousER["ccof_ccofbaseamountoverooscg"]?.Value<decimal?>(),
                ["ccof_ccofbaseamountlesspre"] = PreviousER["ccof_ccofbaseamountlesspre"]?.Value<decimal?>(),
                // CCFRI $
                ["ccof_ccfriamountless0to18"] = PreviousER["ccof_ccfriamountless0to18"]?.Value<decimal?>(),
                ["ccof_ccfriamountover0to18"] = PreviousER["ccof_ccfriamountover0to18"]?.Value<decimal?>(),
                ["ccof_ccfriamountless18to36"] = PreviousER["ccof_ccfriamountless18to36"]?.Value<decimal?>(),
                ["ccof_ccfriamountover18to36"] = PreviousER["ccof_ccfriamountover18to36"]?.Value<decimal?>(),
                ["ccof_ccfriamountless3yk"] = PreviousER["ccof_ccfriamountless3yk"]?.Value<decimal?>(),
                ["ccof_ccfriamountover3yk"] = PreviousER["ccof_ccfriamountover3yk"]?.Value<decimal?>(),
                ["ccof_ccfriamountlessoosck"] = PreviousER["ccof_ccfriamountlessoosck"]?.Value<decimal?>(),
                ["ccof_ccfriamountoveroosck"] = PreviousER["ccof_ccfriamountoveroosck"]?.Value<decimal?>(),
                ["ccof_ccfriamountlessooscg"] = PreviousER["ccof_ccfriamountlessooscg"]?.Value<decimal?>(),
                ["ccof_ccfriamountoverooscg"] = PreviousER["ccof_ccfriamountoverooscg"]?.Value<decimal?>(),
                ["ccof_ccfriamountlesspre"] = PreviousER["ccof_ccfriamountlesspre"]?.Value<decimal?>(),
                //CCFRI Provider $
                ["ccof_ccfriprovideramountless0to18"] = PreviousER["ccof_ccfriprovideramountless0to18"]?.Value<decimal?>(),
                ["ccof_ccfriprovideramountover0to18"] = PreviousER["ccof_ccfriprovideramountover0to18"]?.Value<decimal?>(),
                ["ccof_ccfriprovideramountless18to36"] = PreviousER["ccof_ccfriprovideramountless18to36"]?.Value<decimal?>(),
                ["ccof_ccfriprovideramountover18to36"] = PreviousER["ccof_ccfriprovideramountover18to36"]?.Value<decimal?>(),
                ["ccof_ccfriprovideramountless3yk"] = PreviousER["ccof_ccfriprovideramountless3yk"]?.Value<decimal?>(),
                ["ccof_ccfriprovideramountover3yk"] = PreviousER["ccof_ccfriprovideramountover3yk"]?.Value<decimal?>(),
                ["ccof_ccfriprovideramountlessoosck"] = PreviousER["ccof_ccfriprovideramountlessoosck"]?.Value<decimal?>(),
                ["ccof_ccfriprovideramountoveroosck"] = PreviousER["ccof_ccfriprovideramountoveroosck"]?.Value<decimal?>(),
                ["ccof_ccfriprovideramountlessooscg"] = PreviousER["ccof_ccfriprovideramountlessooscg"]?.Value<decimal?>(),
                ["ccof_ccfriprovideramountoverooscg"] = PreviousER["ccof_ccfriprovideramountoverooscg"]?.Value<decimal?>(),
                ["ccof_ccfriprovideramountlesspre"] = PreviousER["ccof_ccfriprovideramountlesspre"]?.Value<decimal?>(),
                //Grand Total
                ["ccof_grandtotalbase"] = PreviousER["ccof_grandtotalbase"]?.Value<decimal?>(),
                ["ccof_grandtotalccfri"] = PreviousER["ccof_grandtotalccfri"]?.Value<decimal?>(),
                ["ccof_grandtotalccfriprovider"] = PreviousER["ccof_grandtotalccfriprovider"]?.Value<decimal?>(),
                #endregion
                ["ccof_reportextension"] = new JsonObject()
                {
                    // Approved Parent Fee
                    ["ccof_approvedparentfee0to18"] = approvedParentFeesForMonth["ccof_approvedparentfee0to18"]?.DeepClone(),
                    ["ccof_approvedparentfee18to36"] = approvedParentFeesForMonth["ccof_approvedparentfee18to36"]?.DeepClone(),
                    ["ccof_approvedparentfee3yk"] = approvedParentFeesForMonth["ccof_approvedparentfee3yk"]?.DeepClone(),
                    ["ccof_approvedparentfeeoosck"] = approvedParentFeesForMonth["ccof_approvedparentfeeoosck"]?.DeepClone(),
                    ["ccof_approvedparentfeeooscg"] = approvedParentFeesForMonth["ccof_approvedparentfeeooscg"]?.DeepClone(),
                    ["ccof_approvedparentfeepre"] = approvedParentFeesForMonth["ccof_approvedparentfeepre"]?.DeepClone(),
                    ["ccof_approvedparentfeefrequency0to18"] = approvedParentFeesForMonth["ccof_approvedparentfeefrequency0to18"]?.DeepClone(),
                    ["ccof_approvedparentfeefrequency18to36"] = approvedParentFeesForMonth["ccof_approvedparentfeefrequency18to36"]?.DeepClone(),
                    ["ccof_approvedparentfeefrequency3yk"] = approvedParentFeesForMonth["ccof_approvedparentfeefrequency3yk"]?.DeepClone(),
                    ["ccof_approvedparentfeefrequencyoosck"] = approvedParentFeesForMonth["ccof_approvedparentfeefrequencyoosck"]?.DeepClone(),
                    ["ccof_approvedparentfeefrequencyooscg"] = approvedParentFeesForMonth["ccof_approvedparentfeefrequencyooscg"]?.DeepClone(),
                    ["ccof_approvedparentfeefrequencypre"] = approvedParentFeesForMonth["ccof_approvedparentfeefrequencypre"]?.DeepClone(),
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
                ["ccof_dailyenrollment_monthlyenrollmentreport"] = dailyEnrollmentSelected
            };
            _logger.LogInformation(pstTime.ToString("yyyy-MM-dd HH:mm:ss") + " Endpoint: GenerateAdjusementER: EnrolmentReportToCreate json string " + EnrolmentReportToCreate.ToJsonString());
            response = _d365webapiservice.SendCreateRequestAsyncRtn("ccof_monthlyenrollmentreports?$expand=ccof_reportextension,ccof_dailyenrollment_monthlyenrollmentreport", EnrolmentReportToCreate.ToJsonString());
            var content = response.Content.ReadAsStringAsync().Result.ToString();
            JObject returnRecord = new JObject();
            returnRecord = JObject.Parse(content);
            returnRecord = new JObject
            {
                ["ccof_monthlyenrollmentreportid"] = returnRecord["ccof_monthlyenrollmentreportid"]
            };
            var endtime = _timeProvider.GetTimestamp();
            var timediff = _timeProvider.GetElapsedTime(startTime, endtime).TotalSeconds;
            pstTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, PSTZone);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation(pstTime.ToString("yyyy-MM-dd HH:mm:ss") + " Successfully created adjustment ER with ID: " + returnRecord["ccof_monthlyenrollmentreportid"]?.ToString() + " Total time:" + Math.Round(timediff, 2) + " seconds.\r\n");

                return Ok(returnRecord.ToString());
            }
            else
            {
                _logger.LogWarning(pstTime.ToString("yyyy-MM-dd HH:mm:ss") +
                        " Failed to create adjustment ER. Status: {StatusCode}, Reason: {ReasonPhrase}, Content: {Content}. Total processing time: {Duration} seconds.",
                        response.StatusCode,
                        response.ReasonPhrase,
                        content,
                        Math.Round(timediff, 2));
                return StatusCode((int)response.StatusCode, $"Failed to Retrieve records: {response.ReasonPhrase}");
            }
        }
    }
}
