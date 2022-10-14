using CCOF.Infrastructure.WebAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;


namespace CCOF.Infrastructure.WebAPI.Controllers
{
    [Route("api/Authentication")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public class AuthenticationRequestBody
        {
            public string UserName { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
        }

        private class APIUser
        {
            public int UserId { get; private set; }
            public string UserName { get; private set; }
            public string FirstName { get; private set; }
            public string LastName { get; private set; }
            public string BCeID { get; private set; }
            public string Organization { get; private set; }

            public APIUser(
                int userId,
                string userName,
                string firstName,
                string lastName,
                string bceid,
                string org)
            {
                UserId = userId;
                UserName = userName;
                FirstName = firstName;
                LastName = lastName;
                BCeID = bceid;
                Organization = org;
            }
        }

        public AuthenticationController(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        [HttpPost("authenticate")]
        public ActionResult<string> Authenticate(AuthenticationRequestBody authenticationRequestBody)
        {
            // Step 1: validate the username/password
            var user = ValidateUserCredentials(
                authenticationRequestBody.UserName,
                authenticationRequestBody.Password);

            if (user == null)
            {
                return Unauthorized();
            }

            // Step 2: create a token
            return Ok(AcquireToken());
        }

        private APIUser? ValidateUserCredentials(string? userName, string? password)
        {
            // check the passed-through username/password against what's stored in the appsettings.
            var _apiAuthenticationSettingsSection = _configuration.GetSection("CCOFAPISecuritySettings");
            var _apiAuthenticationSettings = _apiAuthenticationSettingsSection.Get<CCOFAPISecuritySettings>();
            var settings = Newtonsoft.Json.JsonConvert.SerializeObject(_apiAuthenticationSettings);

            if (userName != _apiAuthenticationSettings.UserName ||
                password != _apiAuthenticationSettings.Password) { return null; }

            return new APIUser(
                1,
                userName ?? "",
                "Hoang",
                "Le",
                "BCeID12345",
                "Org1");
        }

        public HttpClient GetHttpClient()
        {
            var _dynamicsAuthenticationSettingsSection = _configuration.GetSection("DynamicsAuthenticationSettings");
            var _dynamicsAuthenticationSettings = _dynamicsAuthenticationSettingsSection.Get<DynamicsAuthenticationSettings>();


            AuthenticationParameters ap = AuthenticationParameters.CreateFromUrlAsync(
                new Uri(_dynamicsAuthenticationSettings.CloudResourceUrl)).Result;

            String authorityUrl = ap.Authority;
            String resourceUrl = ap.Resource;

            return getOnlineHttpClient(resourceUrl, authorityUrl, _dynamicsAuthenticationSettings.CloudClientId,
                _dynamicsAuthenticationSettings.CloudClientSecret, _dynamicsAuthenticationSettings.CloudResourceUrl,
                _dynamicsAuthenticationSettings.CloudUserName, _dynamicsAuthenticationSettings.CloudWebApiUrl);

        }

        private HttpClient getOnlineHttpClient(string resourceURI, string authority, string clientId, string clientSecret,
                string redirectUrl, string userName, string webApiUrl)
        {
            HttpClient httpClient = new HttpClient();
            //httpClient.BaseAddress = new Uri(webApiUrl);
            //httpClient.Timeout = new TimeSpan(0, 2, 0);  // 2 minutes  
            //httpClient.DefaultRequestHeaders.Authorization =
            //        new AuthenticationHeaderValue("Bearer", AcquireToken(resourceURI, authority, clientId, clientSecret, redirectUrl, userName));

            return httpClient;
        }

        private string AcquireToken()
        {
            var dynamicsAuthenticationSettingsSection = _configuration.GetSection("DynamicsAuthenticationSettings");
            var dynamicsAuthenticationSettings = dynamicsAuthenticationSettingsSection.Get<DynamicsAuthenticationSettings>();
           
            var serviceUrl = dynamicsAuthenticationSettings.CloudServiceUrl;
            var clientId = dynamicsAuthenticationSettings.CloudClientId;
            var clientSecret = dynamicsAuthenticationSettings.CloudClientSecret;
            //var redirectUri = new Uri(dynamicsAuthenticationSettings.CloudRedirectUrl);

            #region Authentication

            var scope = serviceUrl + "/.default";
            string[] scopes = { scope };

            var clientApp = ConfidentialClientApplicationBuilder.Create(clientId: clientId)
                            .WithClientSecret(clientSecret: clientSecret)
                            //.WithAuthority(new Uri(authority))
                            .Build();

            var authResult = clientApp.AcquireTokenForClient(scopes).ExecuteAsync().Result;


            #endregion Authentication

            #region Client configuration

            var client = new HttpClient
            {
                // See https://docs.microsoft.com/en-us/powerapps/developer/data-platform/webapi/compose-http-requests-handle-errors#web-api-url-and-versions
                BaseAddress = new Uri(dynamicsAuthenticationSettings.CloudWebApiUrl),//resource + "/api/data/v9.2/"),
                Timeout = new TimeSpan(0, 2, 0)    // Standard two minute timeout on web service calls.
            };

            // Default headers for each Web API call.
            // See https://docs.microsoft.com/powerapps/developer/data-platform/webapi/compose-http-requests-handle-errors#http-headers
            HttpRequestHeaders headers = client.DefaultRequestHeaders;
            headers.Authorization = new AuthenticationHeaderValue("Bearer", authResult.AccessToken);
            headers.Add("OData-MaxVersion", "4.0");
            headers.Add("OData-Version", "4.0");
            headers.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            #endregion Client configuration

            #region Web API call

            // Invoke the Web API 'WhoAmI' unbound function.
            // See https://docs.microsoft.com/powerapps/developer/data-platform/webapi/compose-http-requests-handle-errors
            // See https://docs.microsoft.com/powerapps/developer/data-platform/webapi/use-web-api-functions#unbound-functions
            var response = client.GetAsync("WhoAmI").Result;

            if (response.IsSuccessStatusCode)
            {
                // Parse the JSON formatted service response to obtain the user ID.  
                JObject body = JObject.Parse(
                    response.Content.ReadAsStringAsync().Result);
                Guid userId = (Guid)body["UserId"];

                Console.WriteLine("Your user ID is {0}", userId);
            }
            else
            {
                Console.WriteLine("Web API call failed");
                Console.WriteLine("Reason: " + response.ReasonPhrase);
            }
            #endregion Web API call

            return authResult.AccessToken;
        }
    }
}