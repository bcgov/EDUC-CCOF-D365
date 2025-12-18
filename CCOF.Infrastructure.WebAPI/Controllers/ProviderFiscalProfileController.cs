using System.Net;
using System.Threading.Tasks;
using CCOF.Infrastructure.WebAPI.Models;
using CCOF.Infrastructure.WebAPI.Services.D365WebApi;
using CCOF.Infrastructure.WebAPI.Services.D365WebAPI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace CCOF.Infrastructure.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProviderFiscalProfileController : ControllerBase
    {
        private readonly ID365WebApiService _d365webapiservice;
        public ProviderFiscalProfileController(ID365WebApiService d365webapiservice)
        {
            _d365webapiservice = d365webapiservice ?? throw new ArgumentNullException(nameof(d365webapiservice));
        }
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
    <attribute name=""statecode"" />
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
        <attribute name=""ccof_bypass_goodstanding_check""/>
        <attribute name=""ccof_good_standing_status""/>
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
          <attribute name=""ccof_unlock_renewal"" />
          <attribute name=""ccof_application_template_version"" />
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
    <link-entity name=""ofm_portal_role"" from=""ofm_portal_roleid"" to=""ccof_ccof_portal_id"" alias=""PortalRole"" link-type=""outer"">
        <attribute name=""ofm_portal_roleid"" />
        <attribute name=""ofm_portal_role_number"" />
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

                    
                }

                if (records != null && records[0][0]["Organization.accountid"] == null) { return NotFound("No profiles."); }
                if (records != null && records[0][0]["Application.ccof_applicationid"] == null) { return NotFound("No applications."); }

                var aggregatedResult = AggregateApplicationData(records[0][0]);
                return Ok(aggregatedResult);

            }
            else
                return StatusCode((int)response.StatusCode,
                    $"Failed to Retrieve records: {response.Content}");
        }
        private dynamic AggregateApplicationData(JToken token)
        {
            dynamic dynUserProfile = JObject.Parse(token.ToString());
            // exclude Cancelled Facility
            var getFacilitiesStatement = @$"accounts?$select=accountnumber,ccof_healthauthority,name,ccof_formcomplete,accountid,_parentaccountid_value,ccof_facilitystatus,ccof_facilitylicencenumber&$filter=(ccof_accounttype eq 100000001 
and Microsoft.Dynamics.CRM.In(PropertyName='ccof_facilitystatus',PropertyValues=['100000000','100000001','100000002','100000003','100000004','100000005','100000006','100000007','100000008','100000009']) 
and _parentaccountid_value eq {token["Organization.accountid"]})";
            var facilitiesResponse = _d365webapiservice.SendRetrieveRequestAsync(getFacilitiesStatement, true, 250);
            if (facilitiesResponse.IsSuccessStatusCode)
            {
                dynamic jResult = JObject.Parse(facilitiesResponse.Content.ReadAsStringAsync().Result);
                dynUserProfile.facilities = jResult.value;
            }

            var getApplicationStatement = @$"ccof_applications?$select=_ccof_organization_value,ccof_applicationtype,ccof_applicationid,ccof_name,_ccof_programyear_value,statuscode,ccof_providertype,ccof_unlock_declaration,ccof_unlock_licenseupload,ccof_unlock_supportingdocument,ccof_unlock_ccof,ccof_unlock_ecewe,ccof_unlock_renewal,ccof_application_template_version,ccof_licensecomplete,ccof_ecewe_eligibility_complete,ccof_ccofstatus&$expand=ccof_ProgramYear($select=ccof_name,ccof_program_yearid,statuscode,ccof_declarationbstart,ccof_intakeperiodstart,ccof_intakeperiodend),ccof_application_basefunding_Application($select=ccof_application_basefundingid,_ccof_facility_value,statuscode,ccof_formcomplete),ccof_applicationccfri_Application_ccof_ap($select=ccof_applicationccfriid,ccof_ccfrioptin,ccof_name,_ccof_facility_value,statuscode,ccof_formcomplete,ccof_unlock_rfi,ccof_unlock_ccfri,ccof_unlock_nmf_rfi,ccof_has_nmf,ccof_has_rfi,ccof_nmf_formcomplete,ccof_rfi_formcomplete),ccof_ccof_application_ccof_applicationecewe_application($select=ccof_applicationeceweid,ccof_optintoecewe,ccof_name,_ccof_facility_value,statuscode,ccof_formcomplete)&$filter=(_ccof_organization_value eq {token["Organization.accountid"]})";
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

            // A simple way to remove unwanted attributes
            var userProfileString = JsonConvert.SerializeObject(dynUserProfile);
            FiscalUserProfile userProfile = System.Text.Json.JsonSerializer.Deserialize<FiscalUserProfile>(userProfileString);
            

            var portalRoleId = dynUserProfile.Value<string>("PortalRole.ofm_portal_roleid");
            var portalRoleNumber = dynUserProfile.Value<string>("PortalRole.ofm_portal_role_number");

            if (!string.IsNullOrEmpty(portalRoleId) || !string.IsNullOrEmpty(portalRoleNumber))
            {
                userProfile.PortalRole = new PortalRole
                {
                    ofm_portal_roleid = portalRoleId,
                    ofm_portal_role_number = portalRoleNumber
                };
            }
            return userProfile;
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
                var getApplicationCCFRIStatement = @$"ccof_applicationccfris?$select=ccof_applicationccfriid,ccof_ccfrioptin,ccof_closureformcomplete,_ccof_facility_value,ccof_formcomplete,ccof_has_nmf,ccof_has_rfi,ccof_nmf_formcomplete,ccof_rfi_formcomplete,ccof_unlock_ccfri,ccof_unlock_nmf_rfi,ccof_unlock_rfi,ccof_unlock_afsenable,ccof_unlock_afs,ccof_afs_status,statuscode&$expand=ccof_Facility($select=accountid),ccof_Application($select=ccof_applicationid)&$filter=(ccof_Facility/ccof_facilitystatus ne 100000010) and (ccof_Application/ccof_applicationid eq {applicationId})";

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
