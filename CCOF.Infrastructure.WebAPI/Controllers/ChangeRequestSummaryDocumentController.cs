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
    public class ChangeRequestSummaryDocumentController : ControllerBase
    {
        private readonly ID365WebAPIService _d365webapiservice;
        public ChangeRequestSummaryDocumentController(ID365WebAPIService d365webapiservice)
        {
            _d365webapiservice = d365webapiservice ?? throw new ArgumentNullException(nameof(d365webapiservice));
        }
        //Retrieve PDFs based on Change Request id.s
        [HttpGet]
        public ActionResult<string> Get(string changerequestId)
        {
            if (string.IsNullOrEmpty(changerequestId)) return string.Empty;
            var fetchData = new
            {
                ccof_changerequest = changerequestId
            };
            var fetchXML = $@"<?xml version=""1.0"" encoding=""utf-16""?>
                       <fetch version=""1.0"" output-format=""xml-platform"" mapping=""logical"" distinct=""false"">
                              <entity name=""annotation"">
                                <attribute name=""subject"" />
                                <attribute name=""notetext"" />
                                <attribute name=""filename"" />
                                <attribute name=""annotationid"" />
                                <attribute name=""filesize"" />
                                <attribute name=""overriddencreatedon"" />
                                <attribute name=""ownerid"" />
                                <attribute name=""isdocument"" />
                                <attribute name=""createdon"" />
                                <attribute name=""createdby"" />
                                <order attribute=""subject"" descending=""false"" />
                                <link-entity name=""ccof_change_request_summary"" from=""ccof_change_request_summaryid"" to=""objectid"" link-type=""inner"" alias=""ah"">
                                  <attribute name=""ccof_changerequestsummarydocumentname"" />
                                  <attribute name=""ccof_changerequest"" />
                                  <filter type=""and"">
                                    <condition attribute=""ccof_changerequest"" operator=""eq""  value=""{fetchData.ccof_changerequest}"" />
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
        public async Task< ActionResult<string>> UploadFile([FromBody] dynamic value)
        {
            HttpResponseMessage response = null;
            var rawJsonData = value.ToString();
            // stop, if the file size exceeds 3mb 
            if (rawJsonData.Length > 3999999) { return StatusCode((int)HttpStatusCode.InternalServerError, "The file size exceeds the limit allowed (<3Mb)."); };
            JObject obj = JObject.Parse(rawJsonData);
            // obj.Add("notetext", JToken.FromObject(new string("Uploaded Document")));
            rawJsonData = obj.ToString();
            if (obj["ccof_change_requestid"].ToString().Trim() == null)
                return "Change Request ID cannot be empty";

            string filename = (string)obj["filename"];
            string[] partialfilename = filename.Split('.');
            string fileextension = partialfilename[partialfilename.Count() - 1].ToLower();

            // stop, if the file format whether is not JPG, PDF or PNG
            string[] acceptedFileFormats = { "jpg", "jpeg", "pdf", "png", "doc", "docx", "heic", "xls", "xlsx" };

            if (Array.IndexOf(acceptedFileFormats, fileextension) == -1)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, "Sorry, only PDF, JPG and PNG file formats are supported.");
            }
            // check if Change Request  exists.
            var fetchData = new
            {
                ccof_changerequest = obj["ccof_change_requestid"].ToString(),
                
            };
            
          // Create ChangeRequestDocument and a Note with file
            {
                string appUploadFiles = @"
                       {
                          ""ccof_changerequest@odata.bind"": ""/ccof_change_requests(e509ed12-f33d-ee11-bdf4-000d3a09d499)"",
                          ""ccof_change_request_summary_Annotations"": [
                            {
                              ""filename"": ""Congratulations on earning your Microsoft Certification.pdf"",
                              ""filesize"": 281000,
                              ""subject"": ""test 1"",
                              ""documentbody"": """",
                              ""notetext"": ""test"",
                            }
                          ]
                        }"; 
                JObject appUploadFile = new JObject();
                appUploadFile = JObject.Parse(appUploadFiles);
                appUploadFile["ccof_changerequest@odata.bind"] = "/ccof_change_requests(" + obj["ccof_change_requestid"].ToString() + ")";
                appUploadFile["ccof_changerequestsummarydocumentname"] = obj["filename"];
                appUploadFile["ccof_change_request_summary_Annotations"][0]["filename"] = obj["filename"];
                appUploadFile["ccof_change_request_summary_Annotations"][0]["filesize"] = obj["filesize"];
                appUploadFile["ccof_change_request_summary_Annotations"][0]["subject"] = obj["subject"];
                appUploadFile["ccof_change_request_summary_Annotations"][0]["documentbody"] = obj["documentbody"];
                appUploadFile["ccof_change_request_summary_Annotations"][0]["notetext"] = obj["notetext"];
                response = _d365webapiservice.SendCreateRequestAsyncRtn("ccof_change_request_summaries?$expand=ccof_change_request_summary_Annotations($select=subject,filename)", appUploadFile.ToString());
                var content = await response.Content.ReadAsStringAsync();
                JObject returnFile = new JObject();
                returnFile = JObject.Parse(content);

                if (response.IsSuccessStatusCode)
                {
                    return Ok(returnFile["ccof_change_request_summary_Annotations"][0].ToString());
                }
                else
                    return StatusCode((int)response.StatusCode,
                        $"Failed to Retrieve records: {response.ReasonPhrase}");
            }

        }
    }
}
