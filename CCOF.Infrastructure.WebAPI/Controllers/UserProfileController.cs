using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using CCOF.Infrastructure.WebAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

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
<fetch>
  <entity name=""ccof_bceid_organization"">
    <attribute name=""ccof_bceid_organizationid"" />
    <attribute name=""ccof_businessbceid"" />
    <attribute name=""ccof_name"" />
    <attribute name=""ccof_organization"" />
    <link-entity name=""contact"" from=""contactid"" to=""ccof_businessbceid"" link-type=""inner"" alias=""BCeID"">
      <attribute name=""ccof_userid"" />
      <attribute name=""ccof_username"" />
      <filter>
        <condition attribute=""ccof_userid"" operator=""eq"" value=""{userId}"" />
      </filter>
    </link-entity>
    <link-entity name=""account"" from=""accountid"" to=""ccof_organization"" link-type=""inner"" alias=""Organization"">
      <attribute name=""name"" />
      <attribute name=""accountnumber"" />
      <attribute name=""accountid"" />
      <link-entity name=""account"" from=""parentaccountid"" to=""accountid"" alias=""Facility"">
        <attribute name=""name"" />
        <attribute name=""accountnumber"" />
        <attribute name=""ccof_facilitylicencenumber"" />
        <attribute name=""ccof_contractstatus"" />
        <attribute name=""statuscode"" />
        <attribute name=""ccof_facilitystartdate"" />
        <attribute name=""ccof_facilitystatus"" />
        <attribute name=""ccof_sda3percentmedian"" />
        <attribute name=""ccof_servicedeliveryarea"" />
        <link-entity name=""ccof_median_fee_sda"" from=""ccof_median_fee_sdaid"" to=""ccof_organizationcontactname"" link-type=""outer"" alias=""SDAMedian"">
          <attribute name=""ccof_3yearstokindergarten"" />
          <attribute name=""ccof_outofschoolcarekindergarten"" />
          <attribute name=""ccof_preschool"" />
          <attribute name=""ccof_18to36months"" />
          <attribute name=""ccof_0to18months"" />
          <attribute name=""ccof_outofschoolcaregrade1"" />
        </link-entity>
      </link-entity>
      <link-entity name=""ccof_application"" from=""ccof_organization"" to=""accountid"" link-type=""outer"" alias=""Application"">
        <attribute name=""ccof_consent"" />
        <attribute name=""ccof_applicationtype"" />
        <attribute name=""ccof_applicationid"" />
        <attribute name=""ccof_name"" />
        <attribute name=""statuscode"" />
        <attribute name=""ccof_programyear"" />
      </link-entity>
    </link-entity>
  </entity>
</fetch>";

            var message = $"ccof_bceid_organizations?fetchXml=" + WebUtility.UrlEncode(fetchXml);

            var response = _d365webapiservice.SendMessageAsync(HttpMethod.Get, message);
            if (response.IsSuccessStatusCode)
            {
                return Ok(response.Content.ReadAsStringAsync().Result);
            }
            else
                return StatusCode((int)response.StatusCode,
                    $"Failed to Retrieve records: {response.ReasonPhrase}");
        }
    }
}