using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CCOF.Infrastructure.WebAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CCOF.Infrastructure.WebAPI.Controllers
{
    /// <summary>
    /// Wrapper that executes GET (Read), POST (Create), PATCH (Updates), DELETE operations agains the Dyn365 WebApi. View https://docs.microsoft.com/en-us/dynamics365/customer-engagement/developer/use-microsoft-dynamics-365-web-api
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class OperationsController : ControllerBase
    {
        private readonly ID365WebAPIService _d365webapiservice;
        public OperationsController(ID365WebAPIService d365webapiservice)
        {
            _d365webapiservice = d365webapiservice ?? throw new ArgumentNullException(nameof(d365webapiservice));
        }

        /// <summary>
        /// Executes GET operations against the Dyn365 API. View https://docs.microsoft.com/en-us/powerapps/developer/common-data-service/webapi/query-data-web-api
        /// </summary>
        /// <param name="statement">Requested Operation statement</param>
        /// <returns></returns>
        // GET: api/Operations
        [HttpGet]
        public ActionResult<string> Get(string statement, int maxPageSize = 200)
        {
            if (string.IsNullOrEmpty(statement)) return string.Empty;

            if (Request?.QueryString.Value?.IndexOf("&") > 0)
            {
                var filters = Request?.QueryString.Value.Substring(Request.QueryString.Value.IndexOf("&") + 1);
                if (filters?.IndexOf("&maxPageSize") > 0)
                    filters = filters?.Substring(0, filters.IndexOf("&maxPageSize")); //Remove MaxPagesize parameter
                statement = $"{statement}&{filters}";
            }

            var response = _d365webapiservice.SendRetrieveRequestAsync(statement, true, maxPageSize);

            if (response.IsSuccessStatusCode)
                return Ok(response.Content.ReadAsStringAsync().Result);
            else
                return StatusCode((int)response.StatusCode,
                    $"Failed to Retrieve records: {response.ReasonPhrase}");
        }

        /// <summary>
        /// Executes POST operations against the Dyn365 API. View https://docs.microsoft.com/en-us/powerapps/developer/common-data-service/webapi/create-entity-web-api
        /// </summary>
        /// <param name="statement">Requested Operation statement</param>
        /// <param name="value">Json schema with values to be used in the operation</param>
        /// <returns></returns>
        // POST: api/Operations
        [HttpPost]
        public ActionResult<string> Post(string statement, [FromBody] dynamic value)
        {
            if (Request?.QueryString.Value?.IndexOf("&") > 0)
            {
                var filters = Request?.QueryString.Value.Substring(Request.QueryString.Value.IndexOf("&") + 1);
                statement = $"{statement}?{filters}";
            }
            var response = _d365webapiservice.SendCreateRequestAsync(HttpMethod.Post, statement, value.ToString());

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

        /// <summary>
        /// Executes PATCH operations against the Dyn365 API. View https://docs.microsoft.com/en-us/powerapps/developer/common-data-service/webapi/update-delete-entities-using-web-api
        /// </summary>
        /// <param name="statement">Requested Operation statement</param>
        /// <param name="value">Json schema with values to be used in the operation</param>
        /// <returns></returns>
        // PATCH: api/Operations
        [HttpPatch]
        public ActionResult<string> Patch(string statement, [FromBody] dynamic value)
        {
            HttpResponseMessage response = _d365webapiservice.SendUpdateRequestAsync(statement, value.ToString());

            if (response.IsSuccessStatusCode)
                return Ok($"{value.ToString()}");
            else
                return StatusCode((int)response.StatusCode,
                    $"Failed to Update record: {response.ReasonPhrase}");
        }

        /// <summary>
        /// Executes DELETE operations against the Dyn365 API. View https://docs.microsoft.com/en-us/powerapps/developer/common-data-service/webapi/update-delete-entities-using-web-api
        /// </summary>
        /// <param name="statement">Requested Operation statement</param>
        /// <returns></returns>
        // DELETE: api/Operations/5
        [HttpDelete]
        public ActionResult<string> Delete(string statement)
        {
            var response = _d365webapiservice.SendDeleteRequestAsync(statement);

            if (response.IsSuccessStatusCode)
                return Ok($"{statement} removed");
            else
                return StatusCode((int)response.StatusCode,
                    response.Content.ToString());
        }
    }
}