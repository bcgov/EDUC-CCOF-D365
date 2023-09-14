using CCOF.Infrastructure.WebAPI.Models;
using CCOF.Infrastructure.WebAPI.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Net;

namespace CCOF.Infrastructure.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApplicationDetailsController : ControllerBase
    {

        private readonly ID365WebAPIService _d365webapiservice;
        public ApplicationDetailsController(ID365WebAPIService d365webapiservice)
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

            var response = _d365webapiservice.SendMessageAsync(HttpMethod.Get, message).Result;
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
            var applicationResponse = _d365webapiservice.SendRetrieveRequestAsync(getApplicationStatement, true, 250).Result;
            if (applicationResponse.IsSuccessStatusCode)
            {
                dynamic jResult1 = JObject.Parse(applicationResponse.Content.ReadAsStringAsync().Result);
             //   dynamic appWithCR = AppendChangeRequests(jResult1.value[0], token["Application.ccof_applicationid"].ToString());
                dynUserProfile.application = jResult1.value[0];
            }

           
            return dynUserProfile.application;
        }

    }
}
