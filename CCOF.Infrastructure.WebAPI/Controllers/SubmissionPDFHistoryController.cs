using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CCOF.Infrastructure.WebAPI.Models;
using CCOF.Infrastructure.WebAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static System.Net.Mime.MediaTypeNames;

namespace CCOF.Infrastructure.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SubmissionPDFHistoryController : ControllerBase
    {
        private readonly ID365WebAPIService _d365webapiservice;
        public SubmissionPDFHistoryController(ID365WebAPIService d365webapiservice)
        {
            _d365webapiservice = d365webapiservice ?? throw new ArgumentNullException(nameof(d365webapiservice));
        }

        [HttpGet]
        public ActionResult<string> Get(string OrgId)
        {
            if (string.IsNullOrEmpty(OrgId)) return string.Empty;
            var fetchData = new
            {
                orgId = OrgId
            };

            //change Types MTFI - 100000007 , Add New Facility
            var changeRequestfetchXML = $@"<fetch>
  <entity name=""annotation"">
    <attribute name=""annotationid"" />
    <attribute name=""filename"" />
    <attribute name=""filesize"" />
    <attribute name=""isdocument"" />
    <attribute name=""notetext"" />
    <attribute name=""subject"" />
    <link-entity name=""ccof_change_request_summary"" from=""ccof_change_request_summaryid"" to=""objectid"" alias=""cs"">
      <attribute name=""ccof_change_request_summaryid"" />
      <attribute name=""createdon"" />
      <link-entity name=""ccof_change_request"" from=""ccof_change_requestid"" to=""ccof_changerequest"" alias=""cr"">
        <attribute name=""ccof_change_requestid"" />
        <attribute name=""ccof_changetypes"" />
        <attribute name=""ccof_name"" />
        <attribute name=""ccof_organization"" />
        <attribute name=""ccof_program_year"" />
        <filter>
          <condition attribute=""ccof_organization"" operator=""eq"" value=""{fetchData.orgId}"" />
        </filter>
        <link-entity name=""ccof_program_year"" from=""ccof_program_yearid"" to=""ccof_program_year"" alias=""py"">
          <attribute name=""ccof_name"" />
          <attribute name=""ccof_program_yearid"" />
        </link-entity>
      </link-entity>
    </link-entity>
  </entity>
</fetch>";
            var changeRequeststatement = $"annotations?fetchXml=" + WebUtility.UrlEncode(changeRequestfetchXML);
            var changeRequestresponse = _d365webapiservice.SendRetrieveRequestAsync(changeRequeststatement, true).Result;


            var applicationfetchXML = $@"<fetch>
  <entity name=""annotation"">
    <attribute name=""annotationid"" />
    <attribute name=""filename"" />
    <attribute name=""filesize"" />
    <attribute name=""isdocument"" />
    <attribute name=""notetext"" />
    <attribute name=""subject"" />
    <link-entity name=""ccof_applicationsummary"" from=""ccof_applicationsummaryid"" to=""objectid"" alias=""as"">
      <attribute name=""ccof_applicationsummaryid"" />
      <attribute name=""createdon"" />
      <link-entity name=""ccof_application"" from=""ccof_applicationid"" to=""ccof_application"" alias=""app"">
        <attribute name=""ccof_applicationtype"" />
        <attribute name=""ccof_name"" />
        <attribute name=""ccof_programyear"" />
        <filter>
          <condition attribute=""ccof_organization"" operator=""eq"" value=""{fetchData.orgId}"" />
        </filter>
        <link-entity name=""ccof_program_year"" from=""ccof_program_yearid"" to=""ccof_programyear"" alias=""py"">
          <attribute name=""ccof_name"" />
          <attribute name=""ccof_program_yearid"" />
        </link-entity>
      </link-entity>
    </link-entity>
  </entity>
</fetch>";
            var applicationStatement = $"annotations?fetchXml=" + WebUtility.UrlEncode(applicationfetchXML);
            var applicationResponse = _d365webapiservice.SendRetrieveRequestAsync(applicationStatement, true).Result;
            JArray finalResult = new JArray();
            
            if (applicationResponse.IsSuccessStatusCode || changeRequestresponse.IsSuccessStatusCode) {
                ApplicationSummaryDocumentResponse appDocResponse = System.Text.Json.JsonSerializer.Deserialize<ApplicationSummaryDocumentResponse>(applicationResponse.Content.ReadAsStringAsync().Result);
                ChangeRequestDocumentResponse changeRequestDocResponse = System.Text.Json.JsonSerializer.Deserialize<ChangeRequestDocumentResponse>(changeRequestresponse.Content.ReadAsStringAsync().Result);
                JObject appSummaryDocumentResult = JObject.Parse(applicationResponse.Content.ReadAsStringAsync().Result.ToString());

                JArray appSummaryDoc = new JArray();
                appSummaryDoc = appSummaryDocumentResult["value"].ToObject<JArray>();
                if (appSummaryDoc.Count > 0)
                {
                    for (int i = 0; i < appSummaryDoc.Count; i++)
                    {
                        appDocResponse.filename = appSummaryDoc[i]["filename"].ToString();
                        appDocResponse.annotationid = appSummaryDoc[i]["annotationid"].ToString();
                        appDocResponse.id = appSummaryDoc[i]["app.ccof_name"].ToString();
                        appDocResponse.uploadedon = appSummaryDoc[i]["as.createdon"].ToString();
                        appDocResponse.applicationtype = appSummaryDoc[i]["app.ccof_applicationtype@OData.Community.Display.V1.FormattedValue"].ToString();
                        appDocResponse.programyear = appSummaryDoc[i]["app.ccof_programyear@OData.Community.Display.V1.FormattedValue"].ToString();

                        finalResult.Add(new JObject { { "filename", appDocResponse.filename }, { "annotationid", appDocResponse.annotationid }, { "id", appDocResponse.id }, { "uploadedon", appDocResponse.uploadedon }, { "applicationtype", appDocResponse.applicationtype }, { "programyear", appDocResponse.programyear } });
                    }
                }

                JObject changeRequestSummaryDocumentResult = JObject.Parse(changeRequestresponse.Content.ReadAsStringAsync().Result.ToString());
                JArray changeRequestSummaryDoc = new JArray();
                changeRequestSummaryDoc = changeRequestSummaryDocumentResult["value"].ToObject<JArray>();
                if (changeRequestSummaryDoc.Count > 0)
                {
                    for (int i = 0; i < changeRequestSummaryDoc.Count; i++)
                    {


                        changeRequestDocResponse.filename = changeRequestSummaryDoc[i]["filename"].ToString();
                        changeRequestDocResponse.annotationid = changeRequestSummaryDoc[i]["annotationid"].ToString();
                        changeRequestDocResponse.id = changeRequestSummaryDoc[i]["cr.ccof_name"].ToString();
                        changeRequestDocResponse.uploadedon = changeRequestSummaryDoc[i]["cs.createdon"].ToString();
                        if (changeRequestSummaryDoc[i]["cr.ccof_changetypes"].ToString() == "100000007")
                        {
                            changeRequestDocResponse.applicationtype = "MTFI";
                        }
                        else
                        {
                            changeRequestDocResponse.applicationtype = "Change Request";
                        }

                        changeRequestDocResponse.programyear = changeRequestSummaryDoc[i]["cr.ccof_program_year@OData.Community.Display.V1.FormattedValue"].ToString();
                        finalResult.Add(new JObject { { "filename", changeRequestDocResponse.filename }, { "annotationid", changeRequestDocResponse.annotationid }, { "id", changeRequestDocResponse.id }, { "uploadedon", changeRequestDocResponse.uploadedon }, { "applicationtype", changeRequestDocResponse.applicationtype }, { "programyear", changeRequestDocResponse.programyear } });
                    }
                }

                return Ok(finalResult);

            }

            else
                return StatusCode((int)applicationResponse.StatusCode,
                    $"Failed to Retrieve records: {applicationResponse.ReasonPhrase}");
        }
    }
}






