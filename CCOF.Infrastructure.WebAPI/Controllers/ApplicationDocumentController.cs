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
    public class ApplicationDocumentController : ControllerBase
    {
        private readonly ID365WebAPIService _d365webapiservice;
        public ApplicationDocumentController(ID365WebAPIService d365webapiservice)
        {
            _d365webapiservice = d365webapiservice ?? throw new ArgumentNullException(nameof(d365webapiservice));
        }

        [HttpGet]
        public ActionResult<string> Get(string applicationId, int maxPageSize = 1000)
        {
            if (string.IsNullOrEmpty(applicationId)) return string.Empty;
            var fetchData = new
            {
                ccof_application = applicationId
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
                            <link-entity name=""ccof_application_facility_document"" from=""ccof_application_facility_documentid"" to=""objectid"" alias=""ApplicationFacilityDocument"">
                              <attribute name=""ccof_application_facility_documentid"" />
                              <attribute name=""ccof_name"" />
                              <attribute name=""ccof_facility"" />
                              <filter>
                                <condition attribute=""ccof_application"" operator=""eq"" value=""{fetchData.ccof_application}"" uitype=""ccof_application"" />
                              </filter>
                            </link-entity>
                          </entity>
                        </fetch>";
            var statement = $"annotations?fetchXml=" + WebUtility.UrlEncode(fetchXML);
            var response = _d365webapiservice.SendRetrieveRequestAsync(statement, true, maxPageSize);
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
            if (obj["ccof_applicationid"].ToString().Trim() == null || obj["ccof_facility"].ToString().Trim() == null)
                return "Application ID or Facility ID cannot be empty";

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
                ccof_facility = obj["ccof_facility"].ToString()
            };
            var fetchXml = $@"<?xml version=""1.0"" encoding=""utf-16""?>
                    <fetch>
                      <entity name=""ccof_application_facility_document"">
                        <attribute name=""ccof_application"" />
                        <attribute name=""ccof_application_facility_documentid"" />
                        <attribute name=""ccof_name"" />
                        <attribute name=""ccof_facility"" />
                        <filter>
                          <condition attribute=""ccof_application"" operator=""eq"" value=""{fetchData.ccof_application/*cf5fe675-334b-ed11-bba2-000d3af4f80b*/}"" uiname=""APP-22000060"" uitype=""ccof_application"" />
                          <condition attribute=""ccof_facility"" operator=""eq"" value=""{fetchData.ccof_facility/*6f5ade09-6c35-ed11-9db1-002248d53d53*/}"" />
                        </filter>
                      </entity>
                    </fetch>";
            var statement = $"ccof_application_facility_documents?fetchXml=" + WebUtility.UrlEncode(fetchXml);
            response = _d365webapiservice.SendRetrieveRequestAsync(statement, true);
            JObject appFacilityDocsResult = JObject.Parse(response.Content.ReadAsStringAsync().Result.ToString());
            JArray appFacilityDoc = new JArray();
            appFacilityDoc = appFacilityDocsResult["value"].ToObject<JArray>();
            if (appFacilityDoc.Count > 0)   // exist Create a Note with file and associate to ApplicationFaclityDocument table
            {
                string uploadFilestr = @"{
                                    ""filename"":"""",
                                    ""filesize"":0,
                                    ""subject"":"""",
                                    ""notetext"":"""",
                                    ""documentbody"":"""",
                                    ""objectid_ccof_application_facility_document@odata.bind"":"""",
                                    }";
                JObject uploadFile = new JObject();
                uploadFile = JObject.Parse(uploadFilestr);
                uploadFile["filename"] = obj["filename"];
                uploadFile["filesize"] = obj["filesize"];
                uploadFile["subject"] = obj["subject"];
                uploadFile["documentbody"] = obj["documentbody"];
                uploadFile["notetext"] = obj["notetext"];
                uploadFile["objectid_ccof_application_facility_document@odata.bind"] = "/ccof_application_facility_documents(" + appFacilityDoc[0]["ccof_application_facility_documentid"].ToString() + ")";
                response = _d365webapiservice.SendCreateRequestAsyncRtn("annotations?$select=subject,filename", uploadFile.ToString());
                if (response.IsSuccessStatusCode)
                {
                    ApplicationDocumentResponse appDocResponse = System.Text.Json.JsonSerializer.Deserialize<ApplicationDocumentResponse>(response.Content.ReadAsStringAsync().Result);
                    appDocResponse.applicationFacilityDocumentId = appFacilityDoc[0]["ccof_application_facility_documentid"].ToString();
                                       
                    return Ok(appDocResponse);
                }
                else
                    return StatusCode((int)response.StatusCode,
                        $"Failed to Retrieve records: {response.ReasonPhrase}");
            }
            else  // Create ApplicationFaclityDocument and a Note with file
            {
                string appUploadFiles = @"
                       {
                          ""ccof_facility@odata.bind"": ""/accounts(6f5ade09-6c35-ed11-9db1-002248d53d53)"",
                          ""ccof_application@odata.bind"": ""/ccof_applications(cf5fe675-334b-ed11-bba2-000d3af4f80b)"",
                          ""ccof_application_facility_document_Annotations"": [
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
                appUploadFile["ccof_facility@odata.bind"] = "/accounts(" + obj["ccof_facility"].ToString() + ")";
                appUploadFile["ccof_application@odata.bind"] = "/ccof_applications(" + obj["ccof_applicationid"].ToString() + ")";
                appUploadFile["ccof_application_facility_document_Annotations"][0]["filename"] = obj["filename"];
                appUploadFile["ccof_application_facility_document_Annotations"][0]["filesize"] = obj["filesize"];
                appUploadFile["ccof_application_facility_document_Annotations"][0]["subject"] = obj["subject"];
                appUploadFile["ccof_application_facility_document_Annotations"][0]["documentbody"] = obj["documentbody"];
                appUploadFile["ccof_application_facility_document_Annotations"][0]["notetext"] = obj["notetext"];
                response = _d365webapiservice.SendCreateRequestAsyncRtn("ccof_application_facility_documents?$expand=ccof_application_facility_document_Annotations($select=subject,filename)", appUploadFile.ToString());
                JObject returnFile = new JObject();
                returnFile = JObject.Parse(response.Content.ReadAsStringAsync().Result);

                if (response.IsSuccessStatusCode)
                {
                    ApplicationDocumentResponse appDocResponse = System.Text.Json.JsonSerializer.Deserialize<ApplicationDocumentResponse>(returnFile["ccof_application_facility_document_Annotations"][0].ToString());
                    appDocResponse.applicationFacilityDocumentId = returnFile["ccof_application_facility_documentid"].ToString();

                    return Ok(appDocResponse);

                }
                else
                    return StatusCode((int)response.StatusCode,
                        $"Failed to Retrieve records: {response.ReasonPhrase}");
            }

        }
    }
}