
using CCOF.Infrastructure.WebAPI.Models;
using Microsoft.Identity.Client;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using System.Text;

namespace CCOF.Infrastructure.WebAPI.Services
{
    /// <summary>
    /// New and preferred Authentication Service with MSAL library
    /// </summary>
    public class AuthenticationServiceMSAL : ID365AuthenticationService
    {
        private readonly IConfiguration _configuration;
        private readonly D365AuthSettings _authSettings;

        public AuthenticationServiceMSAL(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

            var authSettingsSection = _configuration.GetSection("DynamicsAuthenticationSettings");
            _authSettings = authSettingsSection.Get<D365AuthSettings>();
        } 

        public async Task<HttpClient> GetHttpClient()
        {
            // Get the access token that is required for authentication.
            var accessToken = await GetAccessToken(_authSettings.BaseUrl,
                                                    _authSettings.ClientId,
                                                    _authSettings.ClientSecret,
                                                    _authSettings.TenantId);
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
                                                    _authSettings.ClientSecret,
                                                    _authSettings.TenantId);

            var endpoint = isSearch ? $"{_authSettings.BaseUrl}api/search/{_authSettings.SearchVersion}/query" : _authSettings.WebApiUrl;

            HttpClient client = new()
            {
                BaseAddress = new Uri(endpoint),
                Timeout = new TimeSpan(0, 2, 0)  // 2 minutes
            };
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            return client;
        }

        private static async Task<string> GetAccessToken(string baseUrl, string clientId, string clientSecret, string tenantId)
        {
            string[] scopes = { baseUrl + "/.default" };
            string authority = $"https://login.microsoftonline.com/{tenantId}";

            var clientApp = ConfidentialClientApplicationBuilder.Create(clientId: clientId)
                                                      .WithClientSecret(clientSecret: clientSecret)
                                                      .WithAuthority(new Uri(authority))
                                                      .Build();

            var builder = clientApp.AcquireTokenForClient(scopes);
            var result = await builder.ExecuteAsync();

            return result.AccessToken;         
        }
    }
}
