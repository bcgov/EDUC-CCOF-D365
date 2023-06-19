using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using CCOF.Infrastructure.WebAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Xrm.Sdk;
using Newtonsoft.Json.Linq;
using static System.Net.Mime.MediaTypeNames;

namespace CCOF.Infrastructure.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChangeActionDocumentController : ControllerBase
    {
        private readonly ID365WebAPIService _d365webapiservice;
        public ChangeActionDocumentController(ID365WebAPIService d365webapiservice)
        {
            _d365webapiservice = d365webapiservice ?? throw new ArgumentNullException(nameof(d365webapiservice));
        }
        [HttpGet]
        public ActionResult<string> Get(string changeactionId)
        {
            if (string.IsNullOrEmpty(changeactionId)) return string.Empty;
            var fetchData = new
            {
                ccof_changeaction = changeactionId
            };
            var fetchXML = $@"<?xml version=""1.0"" encoding=""utf-16""?>
                        <fetch>
                          <entity name=""annotation"">
                            <attribute name=""filename"" />
                            <attribute name=""filesize"" />
                            <attribute name=""objectid"" />
                            <attribute name=""stepid"" />
                            <attribute name=""notetext"" />
                            <attribute name=""subject"" />
                            <attribute name=""isdocument"" />
                            <attribute name=""objecttypecode"" />
                            <attribute name=""annotationid"" />
                            <link-entity name=""ccof_change_action"" from=""ccof_change_actionid"" to=""objectid"" alias=""ChangeAction"">
                              <attribute name=""ccof_change_actionid"" />
                              <attribute name=""ccof_name"" />
                              <filter>
                                <condition attribute=""ccof_change_actionid"" operator=""eq"" value=""{fetchData.ccof_changeaction}""/>
                              </filter>
                            </link-entity>
                          </entity>
                        </fetch>";
            var statement = $"annotations?fetchXml=" + WebUtility.UrlEncode(fetchXML);
            var response = _d365webapiservice.SendRetrieveRequestAsync(statement, true);
            if (response.IsSuccessStatusCode)
            {
                return Ok(response.Content.ReadAsStringAsync().Result);
            }
            else
                return StatusCode((int)response.StatusCode,
                    $"Failed to Retrieve records: {response.ReasonPhrase}");
        }

        // UploadFile: api/Document
        [HttpPost]
        [ActionName("UploadFile")]
        public ActionResult<string> UploadFile([FromBody] dynamic value)
        {
            HttpResponseMessage response = null;
            var rawJsonData = value.ToString();
            // stop, if the file size exceeds 3mb 
            if (rawJsonData.Length > 3999999) { return StatusCode((int)HttpStatusCode.InternalServerError, "The file size exceeds the limit allowed (<3Mb)."); };
            JObject obj = JObject.Parse(rawJsonData);
           
            rawJsonData = obj.ToString();
            //Check change request is not null
            if (obj["ccof_change_requestid"].ToString().Trim() == null)
                return "CHANGE REQUEST ID cannot be empty";

            string filename = (string)obj["filename"];
            string[] partialfilename = filename.Split('.');
            string fileextension = partialfilename[partialfilename.Count() - 1].ToLower();

            // stop, if the file format is not JPG, PDF or PNG
            string[] acceptedFileFormats = { "jpg", "jpeg", "pdf", "png", "doc", "docx", "heic", "xls", "xlsx" };

            if (Array.IndexOf(acceptedFileFormats, fileextension) == -1)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, "Sorry, only PDF, JPG and PNG file formats are supported.");
            }
            // check if change action exists based on change request id passed.
            var fetchData = new
            {
                ccof_change_request = obj["ccof_change_requestid"].ToString(),
              
            };
            var fetchXml = $@"<?xml version=""1.0"" encoding=""utf-16""?>
      <fetch>
  <entity name=""ccof_change_action"">
    <attribute name=""ccof_change_actionid"" />
    <attribute name=""ccof_name"" />
    <attribute name=""createdon"" />
    <attribute name=""statuscode"" />
    <attribute name=""statecode"" />
    <attribute name=""ccof_regarding"" />
    <attribute name=""ccof_changetype"" />
    <attribute name=""ccof_change_request"" />
    <order attribute=""ccof_name"" descending=""false"" />
    <filter type=""and"">
      <condition attribute=""ccof_change_request"" operator=""eq"" value=""{fetchData.ccof_change_request}"" />
    </filter>
  </entity>
</fetch>";


            var statement = $"ccof_change_actions?fetchXml=" + WebUtility.UrlEncode(fetchXml);
            response = _d365webapiservice.SendRetrieveRequestAsync(statement, true);
            JObject changeActionDocsResult = JObject.Parse(response.Content.ReadAsStringAsync().Result.ToString());
            JArray changeActionDoc = new JArray();
            changeActionDoc = changeActionDocsResult["value"].ToObject<JArray>();
            // if change action exist, then create a Note with uploaded file and associate it to change action table.
            if (changeActionDoc.Count > 0)  
            {
                string uploadFilestr = @"{
                                    ""filename"":"""",
                                    ""filesize"":0,
                                    ""subject"":"""",
                                    ""notetext"":"""",
                                    ""documentbody"":"""",
                                    ""objectid_ccof_change_action@odata.bind"":"""",
                                    }";
                JObject uploadFile = new JObject();
                uploadFile = JObject.Parse(uploadFilestr);
                uploadFile["filename"] = obj["filename"];
                uploadFile["filesize"] = obj["filesize"];
                uploadFile["subject"] = obj["subject"];
                uploadFile["documentbody"] = obj["documentbody"];
                uploadFile["notetext"] = obj["notetext"];
                uploadFile["objectid_ccof_change_action@odata.bind"] = "/ccof_change_actions(" + changeActionDoc[0]["ccof_change_actionid"].ToString() + ")";
                response = _d365webapiservice.SendCreateRequestAsyncRtn("annotations?$select=subject,filename", uploadFile.ToString());
                if (response.IsSuccessStatusCode)
                {
                    return Ok(response.Content.ReadAsStringAsync().Result);
                }
                else
                    return StatusCode((int)response.StatusCode,
                        $"Failed to Retrieve records: {response.ReasonPhrase}");
            }
            else  // Create change action and a Note with uploaded file
            {
                string changeactionUploadFiles = @"
                       {
                          ""ccof_change_request@odata.bind"": ""/ccof_change_requests(cf5fe675-334b-ed11-bba2-000d3af4f80b)"",
                          ""ccof_change_action_Annotations"": [
                            {
                              ""filename"": ""TESTChangeActionFile.pdf"",
                              ""filesize"": 281000,
                              ""subject"": ""test 1"",
                              ""documentbody"": """",
                              ""notetext"": ""test"",
                            }
                          ]
                        }";
                JObject changeactionUploadFile = new JObject();
                changeactionUploadFile = JObject.Parse(changeactionUploadFiles);
                changeactionUploadFile["ccof_change_request@odata.bind"] = "/ccof_change_requests(" + obj["ccof_change_requestid"].ToString() + ")";
                changeactionUploadFile["ccof_changetype"] = 100000013;
                changeactionUploadFile["ccof_change_action_Annotations"][0]["filename"] = obj["filename"];
                changeactionUploadFile["ccof_change_action_Annotations"][0]["filesize"] = obj["filesize"];
                changeactionUploadFile["ccof_change_action_Annotations"][0]["subject"] = obj["subject"];
                changeactionUploadFile["ccof_change_action_Annotations"][0]["documentbody"] = obj["documentbody"];
                changeactionUploadFile["ccof_change_action_Annotations"][0]["notetext"] = obj["notetext"];
                response = _d365webapiservice.SendCreateRequestAsyncRtn("ccof_change_actions?$expand=ccof_change_action_Annotations($select=subject,filename)", changeactionUploadFile.ToString());
                JObject returnFile = new JObject();
                returnFile = JObject.Parse(response.Content.ReadAsStringAsync().Result);

                if (response.IsSuccessStatusCode)
                {
                    return Ok(returnFile["ccof_change_action_Annotations"][0].ToString());
                }
                else
                    return StatusCode((int)response.StatusCode,
                        $"Failed to Retrieve records: {response.ReasonPhrase}");
            }

        }
    }
}
