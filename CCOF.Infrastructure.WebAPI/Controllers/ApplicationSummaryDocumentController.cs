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
    public class ApplicationSummaryDocumentController : ControllerBase
    {
        private readonly ID365WebAPIService _d365webapiservice;
        public ApplicationSummaryDocumentController(ID365WebAPIService d365webapiservice)
        {
            _d365webapiservice = d365webapiservice ?? throw new ArgumentNullException(nameof(d365webapiservice));
        }
        //Retrieve PDFs based on application id.s
        [HttpGet]
        public ActionResult<string> Get(string applicationId)
        {
            if (string.IsNullOrEmpty(applicationId)) return string.Empty;
            var fetchData = new
            {
                ccof_application = applicationId
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
    <link-entity name=""ccof_applicationsummary"" from=""ccof_applicationsummaryid"" to=""objectid"" link-type=""inner"" alias=""ah"">
      <attribute name=""ccof_applicationsummarydocumentname"" />
      <attribute name=""ccof_application"" />
      <filter type=""and"">
        <condition attribute=""ccof_application"" operator=""eq""  value=""{fetchData.ccof_application}"" />
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
            // obj.Add("notetext", JToken.FromObject(new string("Uploaded Document")));
            rawJsonData = obj.ToString();
            if (obj["ccof_applicationid"].ToString().Trim() == null)
                return "Application ID cannot be empty";

            string filename = (string)obj["filename"];
            string[] partialfilename = filename.Split('.');
            string fileextension = partialfilename[partialfilename.Count() - 1].ToLower();

            // stop, if the file format whether is not JPG, PDF or PNG
            string[] acceptedFileFormats = { "jpg", "jpeg", "pdf", "png", "doc", "docx", "heic", "xls", "xlsx" };

            if (Array.IndexOf(acceptedFileFormats, fileextension) == -1)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, "Sorry, only PDF, JPG and PNG file formats are supported.");
            }
            // check if Facility and Application  exists.
            var fetchData = new
            {
                ccof_application = obj["ccof_applicationid"].ToString(),
                
            };
            
          // Create ApplicationFaclityDocument and a Note with file
            {
                string appUploadFiles = @"
                       {
                          ""ccof_Application@odata.bind"": ""/ccof_applications(cf5fe675-334b-ed11-bba2-000d3af4f80b)"",
                          ""ccof_applicationsummary_Annotations"": [
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
                appUploadFile["ccof_Application@odata.bind"] = "/ccof_applications(" + obj["ccof_applicationid"].ToString() + ")";
                appUploadFile["ccof_applicationsummarydocumentname"] = obj["filename"];
                appUploadFile["ccof_applicationsummary_Annotations"][0]["filename"] = obj["filename"];
                appUploadFile["ccof_applicationsummary_Annotations"][0]["filesize"] = obj["filesize"];
                appUploadFile["ccof_applicationsummary_Annotations"][0]["subject"] = obj["subject"];
                appUploadFile["ccof_applicationsummary_Annotations"][0]["documentbody"] = obj["documentbody"];
                appUploadFile["ccof_applicationsummary_Annotations"][0]["notetext"] = obj["notetext"];
                response = _d365webapiservice.SendCreateRequestAsyncRtn("ccof_applicationsummaries?$expand=ccof_applicationsummary_Annotations($select=subject,filename)", appUploadFile.ToString());
                JObject returnFile = new JObject();
                returnFile = JObject.Parse(response.Content.ReadAsStringAsync().Result);

                if (response.IsSuccessStatusCode)
                {
                    return Ok(returnFile["ccof_applicationsummary_Annotations"][0].ToString());
                }
                else
                    return StatusCode((int)response.StatusCode,
                        $"Failed to Retrieve records: {response.ReasonPhrase}");
            }

        }
    }
}
