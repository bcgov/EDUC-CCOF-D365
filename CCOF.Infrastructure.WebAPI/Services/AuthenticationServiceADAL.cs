
using CCOF.Infrastructure.WebAPI.Models;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using System.Text;

namespace CCOF.Infrastructure.WebAPI.Services
{
    /// <summary>
    /// Authentication Service with older MS ADAL library
    /// </summary>
    public class AuthenticationServiceADAL : ID365AuthenticationService
    {
        private readonly IConfiguration _configuration;
        private readonly D365AuthSettings _authSettings;

        public AuthenticationServiceADAL(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

            var authSettingsSection = _configuration.GetSection("DynamicsAuthenticationSettings");
            _authSettings = authSettingsSection.Get<D365AuthSettings>();
        }

        //public HttpClient GetHttpClient()
        //{
        //    var authSettingsSection = _configuration.GetSection("DynamicsAuthenticationSettings");
        //    var authSettings = authSettingsSection.Get<DynamicsAuthenticationSettings>();

        //    var webApiUrl = $"{authSettings.BaseUrl}/api/data/v{authSettings.APIVersion}/";
        //    _ = AuthenticationParameters.CreateFromResourceUrlAsync(
        //                        new Uri(authSettings.ResourceUrl)).Result;

        //    HttpClient httpClient = new()
        //    {
        //        BaseAddress = new Uri(webApiUrl),
        //        Timeout = new TimeSpan(0, 2, 0)  // 2 minutes 
        //    };
        //    //httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AcquireToken(authSettings.CloudServiceUrl, authSettings.CloudTenantId, authSettings.ClientId, authSettings.CloudClientSecret));

        //    return httpClient;
        //}

        public async Task<HttpClient> GetHttpClient()
        {
            // Get the access token that is required for authentication.
            var accessToken = await GetAccessToken(_authSettings.BaseUrl,
                                                    _authSettings.ClientId,
                                                    _authSettings.ClientSecret);
            HttpClient client = new()
            {
                BaseAddress = new Uri(_authSettings.WebApiUrl),
                Timeout = new TimeSpan(0, 2, 0)  // 2 minutes
            };
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            return client;
        }

        public async Task<HttpClient> GetHttpClient(bool isSearch = false)
        {
            // Get the access token that is required for authentication.
            var accessToken = await GetAccessToken(_authSettings.BaseUrl,
                                                    _authSettings.ClientId,
                                                    _authSettings.ClientSecret);

            var endpoint = isSearch ? $"{_authSettings.BaseUrl}api/search/{_authSettings.SearchVersion}/query" : _authSettings.WebApiUrl;

            HttpClient client = new()
            {
                BaseAddress = new Uri(endpoint),
                Timeout = new TimeSpan(0, 2, 0)  // 2 minutes
            };
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            return client;
        }

        private static async Task<string> GetAccessToken(string baseUrl, string clientId, string clientSecret)
        {
            AuthenticationParameters ap = AuthenticationParameters.CreateFromResourceUrlAsync(new Uri($"{baseUrl}/api/data/")).Result;
            ClientCredential credentials = new(clientId, clientSecret);

            AuthenticationContext authContext = new(ap.Authority, false);
            AuthenticationResult result = await authContext.AcquireTokenAsync(ap.Resource, credentials);

            return result.AccessToken;
        }
    }
}