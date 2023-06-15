using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
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
    public class DocumentController : ControllerBase
    {
        private readonly ID365WebAPIService _d365webapiservice;
        public DocumentController(ID365WebAPIService d365webapiservice)
        {
            _d365webapiservice = d365webapiservice ?? throw new ArgumentNullException(nameof(d365webapiservice));
        }

        // GET: api/Document
        [HttpGet]
        public ActionResult<string> Get(string annotationId, int maxPageSize = 1000)
        {
            if (string.IsNullOrEmpty(annotationId)) return string.Empty;
            string fetchXML = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                                  <entity name='annotation' >
                                    <attribute name='filename' />
                                    <attribute name='filesize' />
                                    <attribute name='notetext' />
                                    <attribute name='documentbody' />
                                    <filter>
                                      <condition attribute='annotationid' operator='eq' value= '{" + annotationId + @"}' />
                                    </filter>
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
            var rawJsonData = value.ToString();

            // stop, if the file size exceeds 3mb 
            if (rawJsonData.Length > 3999999) { return StatusCode((int)HttpStatusCode.InternalServerError, "The file size exceeds the limit allowed (<3Mb)."); };
            JObject obj = JObject.Parse(rawJsonData);
            obj.Add("notetext", JToken.FromObject(new string("Uploaded Document")));
            rawJsonData = obj.ToString();


            string filename = (string)obj["filename"];
            string[] partialfilename = filename.Split('.');
            string fileextension = partialfilename[partialfilename.Count() - 1].ToLower();

            // stop, if the file format whether is not JPG, PDF or PNG
            string[] acceptedFileFormats = { "jpg", "jpeg", "pdf", "png", "doc", "docx", "heic", "xls", "xlsx" };

            if (Array.IndexOf(acceptedFileFormats, fileextension) == -1)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, "Uploaded file format is supported.");
            }
            var response = _d365webapiservice.SendCreateRequestAsync("annotations", rawJsonData);
            //TODO: Improve Exception handling
            if (response.IsSuccessStatusCode)
            {
                var entityUri = response.Headers.GetValues("OData-EntityId")[0];
                string pattern = @"(\{){0,1}[0-9a-fA-F]{8}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{12}(\}){0,1}";
                Match m = Regex.Match(entityUri, pattern, RegexOptions.IgnoreCase);
                var newRecordId = string.Empty;
                if (m.Success) { newRecordId = m.Value; return Ok($"{newRecordId}"); }
                else return StatusCode((int)HttpStatusCode.InternalServerError,
                    "Unable to create record at this time");
            }

            else
                return StatusCode((int)response.StatusCode,
                    $"Failed to Create record: {response.ReasonPhrase}");
        }

        // RemoveFile: api/Document
        [HttpDelete]
        [ActionName("RemoveFile")]
        public ActionResult<string> RemoveFile(string annotationid)
        {
            annotationid = "annotations(" + annotationid + ")";
            var response = _d365webapiservice.SendDeleteRequestAsync(annotationid);
            if (response.IsSuccessStatusCode)
            {
                return Ok("The document has been removed");
            }
            else
                return StatusCode((int)response.StatusCode,
                    $"Failed to remove the document: {response.ReasonPhrase}");
        }
    }
}