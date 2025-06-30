using CCOF.Infrastructure.WebAPI.Models;
using CCOF.Infrastructure.WebAPI.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Net;
using CCOF.Infrastructure.WebAPI.Services.D365WebApi;

namespace CCOF.Infrastructure.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApplicationDetailsController : ControllerBase
    {

        private readonly ID365WebApiService _d365webapiservice;
        public ApplicationDetailsController(ID365WebApiService d365webapiservice)
        {
            _d365webapiservice = d365webapiservice ?? throw new ArgumentNullException(nameof(d365webapiservice));
        }

        // GET: api/UserProfile
        [HttpGet]
        public ActionResult<string> Get(string? ApplicationId = null)
        {

            if (string.IsNullOrEmpty(ApplicationId)) return BadRequest("Invalid Request");

            var fetchXml = $@"<fetch version=""1.0"" output-format=""xml-platform"" mapping=""logical"" distinct=""false"">
  <entity name=""ccof_application"">
    <attribute name=""ccof_applicationid"" />
    <attribute name=""ccof_name"" />
    <attribute name=""createdon"" />
    <order attribute=""ccof_name"" descending=""false"" />
    <filter type=""and"">
      <condition attribute=""ccof_applicationid"" operator=""eq""  value=""{ApplicationId}"" />
    </filter>
  </entity>
</fetch>";
            var message = $"ccof_applications?fetchXml=" + WebUtility.UrlEncode(fetchXml);

            var response = _d365webapiservice.SendMessageAsync(HttpMethod.Get, message);
            if (response.IsSuccessStatusCode)
            {
                var root = JToken.Parse(response.Content.ReadAsStringAsync().Result);

                if (!root.Last().First().HasValues) { return NotFound($"Application not found: {ApplicationId}"); }

                var records = root.Last().ToList();




                var aggregatedResult = AggregateApplicationData(records[0][0]);
                return Ok(aggregatedResult);

            }
            else
                return StatusCode((int)response.StatusCode,
                    $"Failed to Retrieve records: {response.ReasonPhrase}");
        }
        //Aggregate application, facilities & change request details.
        private dynamic AggregateApplicationData(JToken token)
        {
            dynamic dynUserProfile = JObject.Parse(token.ToString());


            var getApplicationStatement = @$"ccof_applications?$select=_ccof_organization_value,ccof_applicationtype,ccof_applicationid,ccof_name,_ccof_programyear_value,statuscode,ccof_providertype,ccof_unlock_declaration,ccof_unlock_licenseupload,ccof_unlock_supportingdocument,ccof_unlock_ccof,ccof_unlock_ecewe,ccof_licensecomplete,ccof_ecewe_eligibility_complete,ccof_ccofstatus&$expand=ccof_ProgramYear($select=ccof_name,ccof_program_yearid,statuscode,ccof_declarationbstart,ccof_intakeperiodstart,ccof_intakeperiodend),ccof_application_basefunding_Application($select=ccof_application_basefundingid,_ccof_facility_value,statuscode,ccof_formcomplete),ccof_applicationccfri_Application_ccof_ap($select=ccof_applicationccfriid,ccof_ccfrioptin,ccof_name,_ccof_facility_value,statuscode,ccof_formcomplete,ccof_unlock_rfi,ccof_unlock_ccfri,ccof_unlock_nmf_rfi,ccof_has_nmf,ccof_has_rfi,ccof_nmf_formcomplete,ccof_rfi_formcomplete),ccof_ccof_application_ccof_applicationecewe_application($select=ccof_applicationeceweid,ccof_optintoecewe,ccof_name,_ccof_facility_value,statuscode,ccof_formcomplete)&$filter=(ccof_applicationid eq {token["ccof_applicationid"]})";
            var applicationResponse = _d365webapiservice.SendRetrieveRequestAsync(getApplicationStatement, true, 250);
            if (applicationResponse.IsSuccessStatusCode)
            {
                dynamic jResult1 = JObject.Parse(applicationResponse.Content.ReadAsStringAsync().Result);
                JArray applicationArray = new JArray();
                dynamic appWithECEWE = null;
                dynamic appwithCCOF = null;
                dynamic appwithCCFRI = null;

                applicationArray = jResult1["value"].ToObject<JArray>();

                dynUserProfile.application = jResult1.value;
                for (int i = 0; i < applicationArray.Count; i++)
                {
                    appWithECEWE = AppendApplicationECEWE(applicationArray[i], applicationArray[i]["ccof_applicationid"].ToString());
                    appwithCCOF = AppendApplicationCCOF(applicationArray[i], applicationArray[i]["ccof_applicationid"].ToString());
                    appwithCCFRI = AppendApplicationCCFRI(applicationArray[i], applicationArray[i]["ccof_applicationid"].ToString());
                    dynUserProfile.application[i] = appWithECEWE;
                    dynUserProfile.application[i] = appwithCCOF;
                    dynUserProfile.application[i] = appwithCCFRI;
                }
            }


            return dynUserProfile.application;
        }
        private dynamic AppendApplicationECEWE(dynamic applicationJson, string applicationId)
        {
            if (applicationJson.ccof_ccof_application_ccof_applicationecewe_application.Count != 0)
            {
                var getApplicationECEWEStatement = @$"ccof_applicationecewes?$select=ccof_applicationeceweid,_ccof_facility_value,ccof_formcomplete,ccof_name,ccof_optintoecewe,statuscode&$expand=ccof_Facility($select=accountid),ccof_application($select=ccof_applicationid)&$filter=(ccof_application/ccof_applicationid eq {applicationId} and ccof_Facility/ccof_facilitystatus ne 100000010)";

                var eceWEResponse = _d365webapiservice.SendRetrieveRequestAsync(getApplicationECEWEStatement, true, 5000);
                if (eceWEResponse.IsSuccessStatusCode)
                {
                    dynamic jResult = JObject.Parse(eceWEResponse.Content.ReadAsStringAsync().Result);
                    applicationJson.ccof_ccof_application_ccof_applicationecewe_application = jResult.value;
                }
            }
            return applicationJson;
        }
        private dynamic AppendApplicationCCOF(dynamic applicationJson, string applicationId)
        {
            if (applicationJson.ccof_application_basefunding_Application.Count != 0)
            {
                var getApplicationCCOFStatement = $@"ccof_application_basefundings?$select=ccof_application_basefundingid,_ccof_facility_value,ccof_formcomplete,statuscode&$expand=ccof_Facility($select=accountid,ccof_facilitystatus),ccof_Application($select=ccof_applicationid,ccof_applicationtype)&$filter=(ccof_Facility/ccof_facilitystatus ne 100000010) and (ccof_Application/ccof_applicationid eq {applicationId})";

                var applicationCCOFResponse = _d365webapiservice.SendRetrieveRequestAsync(getApplicationCCOFStatement, true, 5000);
                if (applicationCCOFResponse.IsSuccessStatusCode)
                {
                    dynamic jResult = JObject.Parse(applicationCCOFResponse.Content.ReadAsStringAsync().Result);
                    applicationJson.ccof_application_basefunding_Application = jResult.value;
                }
            }
            return applicationJson;
        }
        private dynamic AppendApplicationCCFRI(dynamic applicationJson, string applicationId)
        {
            if (applicationJson.ccof_applicationccfri_Application_ccof_ap.Count != 0)
            {
                var getApplicationCCFRIStatement = @$"ccof_applicationccfris?$select=ccof_applicationccfriid,ccof_ccfrioptin,_ccof_facility_value,ccof_formcomplete,ccof_has_nmf,ccof_has_rfi,ccof_nmf_formcomplete,ccof_rfi_formcomplete,ccof_unlock_ccfri,ccof_unlock_nmf_rfi,ccof_unlock_rfi,statuscode&$expand=ccof_Facility($select=accountid),ccof_Application($select=ccof_applicationid)&$filter=(ccof_Facility/ccof_facilitystatus ne 100000010) and (ccof_Application/ccof_applicationid eq {applicationId})";

                var applicationCCFRIResponse = _d365webapiservice.SendRetrieveRequestAsync(getApplicationCCFRIStatement, true, 5000);
                if (applicationCCFRIResponse.IsSuccessStatusCode)
                {
                    dynamic jResult = JObject.Parse(applicationCCFRIResponse.Content.ReadAsStringAsync().Result);
                    applicationJson.ccof_applicationccfri_Application_ccof_ap = jResult.value;
                }
            }
            return applicationJson;
        }
    }
}