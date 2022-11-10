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
    public class ApplicationController : ControllerBase
    {
        private readonly ID365WebAPIService _d365webapiservice;
        public ApplicationController(ID365WebAPIService d365webapiservice)
        {
            _d365webapiservice = d365webapiservice ?? throw new ArgumentNullException(nameof(d365webapiservice));
        }

        // GET: api/Application
        [HttpGet]
        public ActionResult<string> Get(string applicationId)
        {
            if (string.IsNullOrEmpty(applicationId)) return BadRequest("Invalid Request");

//            var fetchXml = $@"<?xml version=""1.0"" encoding=""utf-16""?>
//<fetch>
//  <entity name=""ccof_application"">
//    <attribute name=""createdby"" />
//    <attribute name=""ccof_ecewe_confirmation"" />
//    <attribute name=""ccof_providertype"" />
//    <attribute name=""createdon"" />
//    <attribute name=""ccof_submittedby"" />
//    <attribute name=""ccof_ecewe_selecttheapplicablefundingmodel"" />
//    <attribute name=""ccof_name"" />
//    <attribute name=""statuscode"" />
//    <attribute name=""modifiedby"" />
//    <attribute name=""ccof_consent"" />
//    <attribute name=""ccof_ecewe_optintoecewe"" />
//    <attribute name=""ccof_organization"" />
//    <attribute name=""ccof_applicationid"" />
//    <attribute name=""ccof_applicationtype"" />
//    <attribute name=""modifiedon"" />
//    <attribute name=""statecode"" />
//    <attribute name=""ccof_ecewe_selecttheapplicablesector"" />
//    <attribute name=""ccof_programyear"" />
//    <filter>
//      <condition attribute=""ccof_applicationid"" operator=""eq"" value=""{applicationId}"" uitype=""ccof_application"" />
//    </filter>
//    <link-entity name=""ccof_applicationccfri"" from=""ccof_application"" to=""ccof_applicationid"" link-type=""outer"" alias=""AppCCFRI"">
//      <attribute name=""ccof_name"" />
//      <attribute name=""ccof_informationccfri"" />
//      <attribute name=""ccof_facility"" />
//      <attribute name=""ccof_feecorrectccfri"" />
//      <attribute name=""ccof_chargefeeccfri"" />
//      <attribute name=""ccof_applicationccfriid"" />
//      <link-entity name=""ccof_application_ccfri_childcarecategory"" from=""ccof_applicationccfri"" to=""ccof_applicationccfriid"" link-type=""outer"" alias=""CCFRI_CCC"">
//        <attribute name=""ccof_programyear"" />
//        <attribute name=""ccof_name"" />
//        <attribute name=""ccof_application_ccfri_childcarecategoryid"" />
//        <attribute name=""ccof_childcarecategory"" />
//        <attribute name=""ccof_apr"" />
//        <attribute name=""ccof_aug"" />
//      </link-entity>
//    </link-entity>
//    <link-entity name=""ccof_application_basefunding"" from=""ccof_application"" to=""ccof_applicationid"" link-type=""outer"" alias=""ccof"">
//      <attribute name=""ccof_application_basefundingid"" />
//      <attribute name=""ccof_licensetype"" />
//      <attribute name=""ccof_preschoolmaxnumber"" />
//      <attribute name=""ccof_name"" />
//      <attribute name=""ccof_facility"" />
//    </link-entity>
//  </entity>
//</fetch>";

            var fetchData = new
            {
                ccof_applicationid = applicationId
            };
            var fetchXml = $@"<?xml version=""1.0"" encoding=""utf-16""?>
<fetch>
  <entity name=""ccof_application"">
    <attribute name=""ccof_applicationtype"" />
    <attribute name=""ccof_name"" />
    <attribute name=""ccof_ccofstatus"" />
    <attribute name=""createdon"" />
    <attribute name=""ccof_ecewe_selecttheapplicablesector"" />
    <attribute name=""ccof_consent"" />
    <attribute name=""statecode"" />
    <attribute name=""ccof_ecewe_confirmation"" />
    <attribute name=""createdby"" />
    <attribute name=""modifiedby"" />
    <attribute name=""statuscode"" />
    <attribute name=""ccof_providertype"" />
    <filter>
      <condition attribute=""ccof_applicationid"" operator=""eq"" value=""{fetchData.ccof_applicationid/*0a5943bf-324b-ed11-bba1-002248d53d53*/}"" uiname=""APP-22000128"" uitype=""ccof_application"" />
    </filter>
    <link-entity name=""ccof_applicationccfri"" from=""ccof_application"" to=""ccof_applicationid"" link-type=""outer"" alias=""appCCFRI"">
      <attribute name=""ccof_ccfrioptin"" />
      <attribute name=""ccof_name"" />
      <attribute name=""createdon"" />
      <attribute name=""ccof_application"" />
      <attribute name=""ccof_applicationccfriid"" />
      <link-entity name=""ccof_application_ccfri_childcarecategory"" from=""ccof_applicationccfri"" to=""ccof_applicationccfriid"" link-type=""outer"" alias=""appCCFRI.childcare"">
        <attribute name=""ccof_application_ccfri_childcarecategoryid"" />
        <attribute name=""statecode"" />
        <attribute name=""ccof_name"" />
        <attribute name=""ccof_applicationccfri"" />
        <attribute name=""ccof_apr"" />
      </link-entity>
    </link-entity>
    <link-entity name=""ccof_application_basefunding"" from=""ccof_application"" to=""ccof_applicationid"" link-type=""outer"" alias=""ccof"">
      <attribute name=""ccof_30monthtoschoolage4hoursoflessextendedcc"" />
      <attribute name=""ccof_30monthtoschoolagemorethan4hourextended"" />
      <attribute name=""ccof_closedfacilityinapr"" />
      <attribute name=""ccof_name"" />
    </link-entity>
  </entity>
</fetch>";

            var message = $"ccof_applications?fetchXml=" + WebUtility.UrlEncode(fetchXml);

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