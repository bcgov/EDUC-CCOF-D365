using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using CCOF.Infrastructure.WebAPI.Models;
using CCOF.Infrastructure.WebAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CCOF.Infrastructure.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProviderProfileController : ControllerBase
    {
        private readonly ID365WebAPIService _d365webapiservice;
        public ProviderProfileController(ID365WebAPIService d365webapiservice)
        {
            _d365webapiservice = d365webapiservice ?? throw new ArgumentNullException(nameof(d365webapiservice));
        }

        // GET: api/UserProfile
        [HttpGet]
        public ActionResult<string> Get(string userName, string? userId = null)
        {

            if (string.IsNullOrEmpty(userName)) return BadRequest("Invalid Request");

            var fetchXml = $@"<?xml version=""1.0"" encoding=""utf-16""?>
<fetch top=""1"" distinct=""true"" no-lock=""true"">
  <entity name=""contact"">
    <attribute name=""contactid"" />
    <attribute name=""ccof_userid"" />
    <attribute name=""ccof_username"" />
    <filter type=""or"">
      <condition attribute=""ccof_userid"" operator=""eq"" value=""{userId}"" />
      <condition attribute=""ccof_username"" operator=""eq"" value=""{userName}"" />
    </filter>
    <link-entity name=""ccof_bceid_organization"" from=""ccof_businessbceid"" to=""contactid"" link-type=""outer"">
      <link-entity name=""account"" from=""accountid"" to=""ccof_organization"" link-type=""outer"" alias=""Organization"">
        <attribute name=""accountid"" />
        <attribute name=""name"" />
        <attribute name=""accountnumber"" />
        <attribute name=""ccof_contractstatus"" />
        <attribute name=""ccof_formcomplete"" />
        <attribute name=""ccof_fundingagreementnumber""/>
        <link-entity name=""ccof_application"" from=""ccof_organization"" to=""accountid"" link-type=""outer"" alias=""Application"">
          <attribute name=""ccof_organization"" />
          <attribute name=""ccof_applicationtype"" />
          <attribute name=""ccof_applicationid"" />
          <attribute name=""ccof_name"" />
          <attribute name=""ccof_programyear"" />
          <attribute name=""statuscode"" />
          <attribute name=""ccof_providertype"" />
          <attribute name=""ccof_unlock_declaration"" />
          <attribute name=""ccof_unlock_licenseupload"" />
          <attribute name=""ccof_unlock_supportingdocument"" />
          <attribute name=""ccof_unlock_ccof"" />
          <attribute name=""ccof_unlock_ecewe"" />
          <link-entity name=""ccof_program_year"" from=""ccof_program_yearid"" to=""ccof_programyear"" link-type=""outer"" alias=""ProgramYear"">
            <attribute name=""ccof_name"" />
            <attribute name=""ccof_program_yearid"" />
            <attribute name=""statuscode"" />
            <attribute name=""ccof_declarationbstart"" />
            <attribute name=""ccof_intakeperiodstart"" />
            <attribute name=""ccof_intakeperiodend"" />
            <order attribute=""ccof_programyearnumber"" descending=""true"" />
          </link-entity>
        </link-entity>
      </link-entity>
    </link-entity>
  </entity>
</fetch>";
            var message = $"contacts?fetchXml=" + WebUtility.UrlEncode(fetchXml);

            var response = _d365webapiservice.SendMessageAsync(HttpMethod.Get, message);
            if (response.IsSuccessStatusCode)
            {
                var root = JToken.Parse(response.Content.ReadAsStringAsync().Result);

                if (!root.Last().First().HasValues) { return NotFound($"User not found: {userId}"); }

                var records = root.Last().ToList();
                if (records != null && records[0][0]["ccof_userid"] == null && !string.IsNullOrEmpty(userId))
                {
                    // Update Dataverse with the userid
                    var statement = @$"contacts({records[0][0]["contactid"]})";
                    var body = System.Text.Json.JsonSerializer.Serialize(new { ccof_userid = userId });
                    HttpResponseMessage updateRespopnse = _d365webapiservice.SendUpdateRequestAsync(statement, body);

                    if (!updateRespopnse.IsSuccessStatusCode)
                    {
                        Console.WriteLine(StatusCode((int)response.StatusCode));
                    }
                }

                if (records != null && records[0][0]["Organization.accountid"] == null) { return NotFound("No profiles."); }
                if (records != null && records[0][0]["Application.ccof_applicationid"] == null) { return NotFound("No applications."); }

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
            var getFacilitiesStatement = @$"accounts?$select=accountnumber,name,ccof_formcomplete,accountid,_parentaccountid_value,ccof_facilitystatus,ccof_facilitylicencenumber&$filter=(ccof_accounttype eq 100000001 and _parentaccountid_value eq {token["Organization.accountid"]})";
            var facilitiesResponse = _d365webapiservice.SendRetrieveRequestAsync(getFacilitiesStatement, true, 250);
            if (facilitiesResponse.IsSuccessStatusCode)
            {
                dynamic jResult = JObject.Parse(facilitiesResponse.Content.ReadAsStringAsync().Result);
                dynUserProfile.facilities = jResult.value;
            }

            var getApplicationStatement = @$"ccof_applications?$select=_ccof_organization_value,ccof_applicationtype,ccof_applicationid,ccof_name,_ccof_programyear_value,statuscode,ccof_providertype,ccof_unlock_declaration,ccof_unlock_licenseupload,ccof_unlock_supportingdocument,ccof_unlock_ccof,ccof_unlock_ecewe,ccof_licensecomplete,ccof_ecewe_eligibility_complete,ccof_ccofstatus&$expand=ccof_ProgramYear($select=ccof_name,ccof_program_yearid,statuscode,ccof_declarationbstart,ccof_intakeperiodstart,ccof_intakeperiodend),ccof_application_basefunding_Application($select=ccof_application_basefundingid,_ccof_facility_value,statuscode,ccof_formcomplete),ccof_applicationccfri_Application_ccof_ap($select=ccof_applicationccfriid,ccof_ccfrioptin,ccof_name,_ccof_facility_value,statuscode,ccof_formcomplete,ccof_unlock_rfi,ccof_unlock_ccfri,ccof_unlock_nmf_rfi,ccof_has_nmf,ccof_has_rfi,ccof_nmf_formcomplete,ccof_rfi_formcomplete),ccof_ccof_application_ccof_applicationecewe_application($select=ccof_applicationeceweid,ccof_optintoecewe,ccof_name,_ccof_facility_value,statuscode,ccof_formcomplete),ccof_ccof_change_request_Application_ccof_appl($select = ccof_change_requestid, ccof_name)&$filter=(ccof_applicationid eq {token["Application.ccof_applicationid"]})";
            var applicationResponse = _d365webapiservice.SendRetrieveRequestAsync(getApplicationStatement, true, 250);
            if (applicationResponse.IsSuccessStatusCode)
            {
                dynamic jResult1 = JObject.Parse(applicationResponse.Content.ReadAsStringAsync().Result);
                dynamic appWithCR = AppendChangeRequests(jResult1.value[0], token["Application.ccof_applicationid"].ToString());
                dynUserProfile.application = appWithCR;
            }

            // A simple way to remove unwanted attributes
            var userProfileString = JsonConvert.SerializeObject(dynUserProfile);
            UserProfile userProfile = System.Text.Json.JsonSerializer.Deserialize<UserProfile>(userProfileString);

            return userProfile;
        }

        private dynamic AppendChangeRequests(dynamic applicationJson, string applicationId)
        {
            if (applicationJson.ccof_ccof_change_request_Application_ccof_appl.Count != 0)
            {
                var getChangeRequestsStatement = @$"ccof_change_requests?$select=ccof_change_requestid, ccof_name, ccof_unlock_change_request, ccof_unlock_declaration, ccof_unlock_document, statecode, statuscode&$expand=ccof_change_action_change_request($select=ccof_change_actionid,_ccof_change_request_value,ccof_changetype,ccof_name,statecode,statuscode;$expand=ccof_change_request_new_facility_change_act($select=_ccof_ccof_value,_ccof_change_action_value,ccof_change_request_new_facilityid,_ccof_ecewe_value,_ccof_facility_value,statecode,statuscode,_ccof_ccfri_value,ccof_name))&$filter=(_ccof_application_value eq {applicationId})";

                var changeRequestResponse = _d365webapiservice.SendRetrieveRequestAsync(getChangeRequestsStatement, true, 5000);
                if (changeRequestResponse.IsSuccessStatusCode)
                {
                    dynamic jResult = JObject.Parse(changeRequestResponse.Content.ReadAsStringAsync().Result);
                    applicationJson.ccof_ccof_change_request_Application_ccof_appl = jResult.value;
                }
            }
            return applicationJson;
        }
    }
}

