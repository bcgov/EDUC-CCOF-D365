using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CCOF.Infrastructure.WebAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace CCOF.Infrastructure.WebAPI.Controllers
{
    /// <summary>
    /// Controller is used to indicate the health/availability of the service. 
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class EnvironmentInformation : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public EnvironmentInformation(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // GET: api/EnvironmentInformation
        [HttpGet]
        public ActionResult<string> Get()
        {
            var _authSettingsSection = _configuration.GetSection("DynamicsAuthenticationSettings");
            var _authSettings = _authSettingsSection.Get<D365AuthSettings>();

            _authSettings.ClientId = "*********";
            _authSettings.ClientSecret = "*********";
            var settings = Newtonsoft.Json.JsonConvert.SerializeObject(_authSettings);

            return Ok(settings);
        }
    }
}
