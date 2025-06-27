using CCOF.Infrastructure.WebAPI.Extensions;
using CCOF.Infrastructure.WebAPI.Models;
using CCOF.Infrastructure.WebAPI.Services.D365WebApi;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;
using Microsoft.Xrm.Sdk;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using System.Text;

namespace CCOF.Infrastructure.WebAPI.Services.D365WebAPI
{
    /// <summary>
    /// New and preferred Authentication Service with MSAL library
    /// </summary>
    public class AuthenticationServiceMSAL : ID365AuthenticationService
    {
        private readonly D365AuthSettings _authSettings;
        private readonly ID365TokenService _tokenService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
       

        
        public AuthenticationServiceMSAL(IConfiguration configuration, ID365TokenService tokenService, IHttpClientFactory factory)
        {
           _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
            _httpClientFactory = factory;
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

            var authSettingsSection = _configuration.GetSection("D365AuthSettings");
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

        public async Task<HttpClient> GetHttpClientAsync(D365ServiceType requestType, AZAppUser spn)
        {
            var accessToken = await _tokenService.FetchAccessToken(_authSettings.BaseUrl, spn);
            var baseAddress = requestType switch
            {
                D365ServiceType.Search => $"{_authSettings.BaseUrl}api/search/{_authSettings.SearchVersion}/query",
                D365ServiceType.Batch => $"{_authSettings.BaseUrl}api/data/{_authSettings.ApiVersion}/$batch",
                _ => _authSettings.WebApiUrl,
            };
            var client = _httpClientFactory.CreateClient(_authSettings.HttpClientName);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            client.BaseAddress = new Uri(baseAddress);

            return client;
        }
    }
}
