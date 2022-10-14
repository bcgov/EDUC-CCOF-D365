using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CCOF.Infrastructure.WebAPI.Models;
using CCOF.Infrastructure.WebAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CCOF.Infrastructure.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SearchController : ControllerBase
    {
        private readonly ID365WebAPIService _d365webapiservice;
        public SearchController(ID365WebAPIService d365webapiservice)
        {
            _d365webapiservice = d365webapiservice ?? throw new ArgumentNullException(nameof(d365webapiservice));
        }

        [HttpPost]
        public ActionResult<string> Post([FromBody] dynamic search)
        {
            dynamic validJson = ValidateSearchJson(search);
            if (!validJson.IsValid) return BadRequest("Invalid search");

            var validSearch = new DataverseSearch() { search = Regex.Replace(validJson.Search, @"[^\w $\-]", "").Trim() };// Remove special characters

            var response = _d365webapiservice.SendSearchRequestAsync(JsonSerializer.Serialize(validSearch));

            if (response.IsSuccessStatusCode)
            {
                return Ok(response.Content.ReadAsStringAsync().Result);
            }
            else
                return StatusCode((int)response.StatusCode,
                    $"Failed to perform Search: {response.ReasonPhrase}");
        }

        private static dynamic ValidateSearchJson(dynamic search)
        {
            try
            {
                DataverseSearch searchObject = JsonSerializer.Deserialize<DataverseSearch>(search.ToString());
                if (searchObject.search is null || searchObject.search.Length > 100) return false; // Dynamics Dataverse search limit is 100 characters

                return new { Search = searchObject.search, IsValid = true };
            }
            catch
            {
                return new { IsValid = false };
            }
        }
    }
}