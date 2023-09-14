using System.Net.Http.Headers;
using System.Text;

namespace CCOF.Infrastructure.WebAPI.Services
{
    public interface ID365WebAPIService
    {
        Task<HttpResponseMessage> SendRetrieveRequestAsync(string query, bool formatted = false, int maxPageSize = 200);
        Task<HttpResponseMessage> SendCreateRequestAsync(string endPoint, string content);
        Task<HttpResponseMessage> SendCreateRequestAsyncRtn(string endPoint, string content);
        Task<HttpResponseMessage> SendCreateRequestAsync(HttpMethod httpMethod, string entitySetName, string body);
        Task<HttpResponseMessage> SendDeleteRequestAsync(string endPoint);
        Task<HttpResponseMessage> SendUpdateRequestAsync(string endPoint, string content);
        Task<HttpResponseMessage> SendMessageAsync(HttpMethod httpMethod, string messageUri);
        Task<HttpResponseMessage> SendSearchRequestAsync(string body);
    }

    public class D365WebAPIService : ID365WebAPIService
    {
        private readonly ID365AuthenticationService _authenticationService;

        public D365WebAPIService(ID365AuthenticationService authenticationService)
        {
            _authenticationService = authenticationService ?? throw new ArgumentNullException(nameof(authenticationService));
        }

        public async Task<HttpResponseMessage> SendRetrieveRequestAsync(string query, Boolean formatted = false, int maxPageSize = 200)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, query);
            request.Headers.Add("Prefer", "odata.maxpagesize=" + maxPageSize.ToString());
            if (formatted)
                request.Headers.Add("Prefer", "odata.include-annotations=OData.Community.Display.V1.FormattedValue");

            var client = await _authenticationService.GetHttpClient();

            return await client.SendAsync(request);
        }

        public async Task<HttpResponseMessage> SendCreateRequestAsync(string endPoint, string content)
        {
            return await SendAsync(HttpMethod.Post, endPoint, content);
        }

        public async Task<HttpResponseMessage> SendCreateRequestAsyncRtn(string endPoint, string content)
        {
            return await SendAsyncRtn(HttpMethod.Post, endPoint, content);
        }

        public async Task<HttpResponseMessage> SendUpdateRequestAsync(string endPoint, string body)
        {
            var message = new HttpRequestMessage(HttpMethod.Patch, endPoint);
            message.Content = new StringContent(body, Encoding.UTF8, "application/json");

            var client = await _authenticationService.GetHttpClient();

            return await client.SendAsync(message);
        }

        public async Task<HttpResponseMessage> SendDeleteRequestAsync(string endPoint)
        {
            var client = await _authenticationService.GetHttpClient();

            return await client.DeleteAsync(endPoint);
        }

        public async Task<HttpResponseMessage> SendCreateRequestAsync(HttpMethod httpMethod, string entitySetName, string body)
        {
            var message = new HttpRequestMessage(httpMethod, entitySetName);
            message.Content = new StringContent(body, Encoding.UTF8, "application/json");

            var client = await _authenticationService.GetHttpClient();

            return await client.SendAsync(message);
        }

        public async Task<HttpResponseMessage> SendMessageAsync(HttpMethod httpMethod, string messageUri)
        {
            HttpRequestMessage message = new(httpMethod, messageUri);

            var client = await _authenticationService.GetHttpClient();

            return await client.SendAsync(message);
        }

        public async Task<HttpResponseMessage> SendSearchRequestAsync(string body)
        {
            var message = new HttpRequestMessage()
            {
                Content = new StringContent(body, Encoding.UTF8, "application/json")
            };
            message.Method = HttpMethod.Post;

            var client = await _authenticationService.GetHttpClient(isSearch: true);

            return await client.SendAsync(message);
        }

        private async Task<HttpResponseMessage> SendAsync(HttpMethod operation, string endPoint, string body)
        {
            var message = new HttpRequestMessage(operation, endPoint);
            message.Content = new StringContent(body, Encoding.UTF8, "application/json");
            message.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

            var client = await _authenticationService.GetHttpClient();
            return await client.SendAsync(message);
        }

        private async Task<HttpResponseMessage> SendAsyncRtn(HttpMethod operation, string endPoint, string body)
        {
            var message = new HttpRequestMessage(operation, endPoint);
            message.Content = new StringContent(body, Encoding.UTF8, "application/json");
            message.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
            message.Headers.Add("Prefer", "return=representation");

            var client = await _authenticationService.GetHttpClient();
            return await client.SendAsync(message);
        }
    }
}