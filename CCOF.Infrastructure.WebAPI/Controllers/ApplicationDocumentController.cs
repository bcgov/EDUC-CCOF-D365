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
    public class ApplicationDocumentController : ControllerBase
    {
        private readonly ID365WebAPIService _d365webapiservice;
        public ApplicationDocumentController(ID365WebAPIService d365webapiservice)
        {
            _d365webapiservice = d365webapiservice ?? throw new ArgumentNullException(nameof(d365webapiservice));
        }  

        [HttpGet]
        public ActionResult<string> Get(string applicationId)
        {
            if (string.IsNullOrEmpty(applicationId)) return string.Empty;
            string fetchXML = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                                  <entity name='annotation' >
                                    <attribute name='filename' />
                                    <attribute name='filesize' />
                                    <attribute name='notetext' />
                                    <filter>
                                      <condition attribute='objecttypecodename' operator='eq' value='assignment' />
                                      <condition attribute='notetext' operator='eq' value='Uploaded Documents' />
                                      <condition attribute='objectid' operator='eq' value= '{" + applicationId + @"}' />
                                    </filter>                                
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
    }
}