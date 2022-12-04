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
    public class FacilityController : ControllerBase
    {
        private readonly ID365WebAPIService _d365webapiservice;
        public FacilityController(ID365WebAPIService d365webapiservice)
        {
            _d365webapiservice = d365webapiservice ?? throw new ArgumentNullException(nameof(d365webapiservice));
        }

        // GET: api/Facility
        [HttpGet]
        public ActionResult<string> Get(string id)
        {
            if (string.IsNullOrEmpty(id)) return BadRequest("Invalid Request");

            var fetchXml = $@"<?xml version=""1.0"" encoding=""utf-16""?>
<fetch distinct=""true"" no-lock=""true"">
  <entity name=""ccof_facility_licenses"">
    <attribute name=""ccof_name"" />
    <attribute name=""ccof_licensecategory"" />
    <attribute name=""ccof_facility"" />
    <attribute name=""ccof_facility_licensesid"" />
    <link-entity name=""account"" from=""accountid"" to=""ccof_facility"" link-type=""inner"" alias=""Facility"">
      <attribute name=""ccof_accounttype"" />
      <attribute name=""accountnumber"" />
      <attribute name=""accountid"" />
      <attribute name=""name"" />
      <filter>
        <condition attribute=""accountid"" operator=""eq"" value=""{id}"" uitype=""account"" />
      </filter>
    </link-entity>
    <link-entity name=""ccof_license_category"" from=""ccof_license_categoryid"" to=""ccof_licensecategory"" alias=""License"">
      <attribute name=""ccof_name"" />
      <attribute name=""statuscode"" />
      <attribute name=""ccof_categorynumber"" />
      <attribute name=""ccof_license_categoryid"" />
      <link-entity name=""ccof_license_childcare_category"" from=""ccof_licensecategory"" to=""ccof_license_categoryid"" link-type=""inner"">
        <attribute name=""ccof_name"" />
        <attribute name=""ccof_licensecategory"" />
        <attribute name=""ccof_license_childcare_categoryid"" />
        <attribute name=""ccof_childcarecategory"" />
        <link-entity name=""ccof_childcare_category"" from=""ccof_childcare_categoryid"" to=""ccof_childcarecategory"" alias=""CareType"">
            <attribute name=""ccof_childcare_categoryid"" />
            <attribute name=""ccof_childcarecategorynumber"" />
            <attribute name=""ccof_name"" />         
            <attribute name=""statuscode"" />
          <filter type=""and"">
            <condition attribute=""statuscode"" operator=""eq"" value=""1"" />
          </filter>
        </link-entity>
      </link-entity>
    </link-entity>
  </entity>
</fetch>";
            var message = $"ccof_facility_licenseses?fetchXml=" + WebUtility.UrlEncode(fetchXml);

            var response = _d365webapiservice.SendMessageAsync(HttpMethod.Get, message);
            if (response.IsSuccessStatusCode)
            {
                var root = JToken.Parse(response.Content.ReadAsStringAsync().Result);
               
                if (root.Last().First().HasValues)
                {     
                    return Ok(response.Content.ReadAsStringAsync().Result);
                }
                else
                {
                    return NotFound($"No Data: {id}");
                }
            }
            else
                return StatusCode((int)response.StatusCode,
                    $"Failed to Retrieve records: {response.ReasonPhrase}");
        }
    }
}