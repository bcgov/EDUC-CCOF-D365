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
            var response = _d365webapiservice.SendRetrieveRequestAsync(statement, true).Result;
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
            if (obj["ccof_change_action_id"].ToString().Trim() == null)
                return "CHANGE ACTION ID cannot be empty";

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
                ccof_change_action = obj["ccof_change_action_id"].ToString(),
              
            };
           
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
                uploadFile["objectid_ccof_change_action@odata.bind"] = "/ccof_change_actions(" + fetchData.ccof_change_action + ")";
                response = _d365webapiservice.SendCreateRequestAsyncRtn("annotations?$select=subject,filename", uploadFile.ToString()).Result;
                if (response.IsSuccessStatusCode)
                {
                    return Ok(response.Content.ReadAsStringAsync().Result);
                }
                else
                    return StatusCode((int)response.StatusCode,
                        $"Failed to upload file: {response.ReasonPhrase}");
            
           

        }
    }
}
