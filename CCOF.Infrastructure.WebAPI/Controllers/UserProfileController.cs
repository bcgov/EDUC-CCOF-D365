using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using CCOF.Infrastructure.WebAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace CCOF.Infrastructure.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserProfileController : ControllerBase
    {
        private readonly ID365WebAPIService _d365webapiservice;
        public UserProfileController(ID365WebAPIService d365webapiservice)
        {
            _d365webapiservice = d365webapiservice ?? throw new ArgumentNullException(nameof(d365webapiservice));
        }

        // GET: api/UserProfile
        [HttpGet]
        public ActionResult<string> Get(string userId)
        {
            if (string.IsNullOrEmpty(userId)) return BadRequest("Invalid Request");

            var fetchXml = $@"<?xml version=""1.0"" encoding=""utf-16""?>
<fetch distinct=""true"" no-lock=""true"">
  <entity name=""contact"">
    <attribute name=""ccof_userid"" />
    <attribute name=""ccof_username"" />
    <filter>
      <condition attribute=""ccof_userid"" operator=""eq"" value=""{userId}"" />
    </filter>
    <link-entity name=""ccof_bceid_organization"" from=""ccof_businessbceid"" to=""contactid"" link-type=""outer"">
      <attribute name=""ccof_name"" />
      <attribute name=""statuscode"" />
      <link-entity name=""account"" from=""accountid"" to=""ccof_organization"" link-type=""outer"" alias=""Organization"">
        <attribute name=""accountid"" />
        <attribute name=""ccof_contractstatus"" />
        <attribute name=""name"" />
        <attribute name=""accountnumber"" />
        <link-entity name=""ccof_application"" from=""ccof_organization"" to=""accountid"" link-type=""outer"" alias=""Application"">
          <attribute name=""ccof_organization"" />
          <attribute name=""ccof_applicationtype"" />
          <attribute name=""ccof_applicationid"" />
          <attribute name=""ccof_name"" />
          <attribute name=""ccof_programyear"" />
          <attribute name=""statuscode"" />
          <attribute name=""ccof_providertype"" />
          <link-entity name=""ccof_program_year"" from=""ccof_program_yearid"" to=""ccof_programyear"" link-type=""outer"" alias=""ProgramYear"">
            <attribute name=""ccof_name"" />
            <attribute name=""ccof_program_yearid"" />
            <attribute name=""statuscode"" />
            <order attribute=""ccof_name"" descending=""true"" />
          </link-entity>
          <link-entity name=""ccof_application_basefunding"" from=""ccof_application"" to=""ccof_applicationid"" link-type=""outer"" alias=""CCOF"">
            <attribute name=""ccof_facility"" />
            <attribute name=""statuscode"" />
            <attribute name=""ccof_application_basefundingid"" />
            <attribute name=""ccof_formcomplete"" />
            <link-entity name=""account"" from=""accountid"" to=""ccof_facility"" link-type=""outer"" alias=""CCOF.Facility"">
              <attribute name=""name"" />
              <attribute name=""accountnumber"" />
              <attribute name=""ccof_formcomplete"" />
            </link-entity>
          </link-entity>
          <link-entity name=""ccof_applicationccfri"" from=""ccof_application"" to=""ccof_applicationid"" link-type=""outer"" alias=""CCFRI"">
            <attribute name=""ccof_ccfrioptin"" />
            <attribute name=""ccof_name"" />
            <attribute name=""ccof_facility"" />
            <attribute name=""statuscode"" />
            <attribute name=""ccof_applicationccfriid"" />
            <attribute name=""ccof_formcomplete"" />
            <link-entity name=""account"" from=""accountid"" to=""ccof_facility"" link-type=""outer"" alias=""CCFRI.Facility"">
              <attribute name=""accountnumber"" />
              <attribute name=""accountid"" />
              <attribute name=""ccof_formcomplete"" />
            </link-entity>
          </link-entity>
          <link-entity name=""ccof_applicationecewe"" from=""ccof_application"" to=""ccof_applicationid"" link-type=""outer"" alias=""ECEWE"">
            <attribute name=""ccof_applicationeceweid"" />
            <attribute name=""ccof_optintoecewe"" />
            <attribute name=""ccof_name"" />
            <attribute name=""statuscode"" />
            <attribute name=""ccof_formcomplete"" />
            <attribute name=""ccof_facility"" />
            <link-entity name=""account"" from=""accountid"" to=""ccof_applicationeceweid"" link-type=""outer"" alias=""ECEWE.Facility"">
              <attribute name=""name"" />
              <attribute name=""accountnumber"" />
              <attribute name=""ccof_formcomplete"" />
            </link-entity>
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

                if (!root.Last().First().HasValues) { 
                    //Add Search by userName here

                    return NotFound($"User not found: {userId}"); 
                }

                var records = root.Last().ToList();
                if (records != null & records[0][0]["Organization.accountid"] == null) { return NotFound("No profiles."); }
                if (records != null & records[0][0]["Application.ccof_applicationid"] == null) { return NotFound("No applications."); }

                var applicationId = records[0][0]["Application.ccof_applicationid"]; // Latest Application
                var values = records[0].Where(t => (string)t["Application.ccof_applicationid"] == applicationId.ToString());
                return Ok(values);
            }
            else
                return StatusCode((int)response.StatusCode,
                    $"Failed to Retrieve records: {response.ReasonPhrase}");
        }
    }
}