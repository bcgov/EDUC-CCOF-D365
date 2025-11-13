using CCOF.Infrastructure.WebAPI.Extensions;
using CCOF.Infrastructure.WebAPI.Messages;
using CCOF.Infrastructure.WebAPI.Models;
using CCOF.Infrastructure.WebAPI.Services.D365WebApi;
using CCOF.Infrastructure.WebAPI.Services.Processes;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.Json;
using System.Threading.Tasks;
using static CCOF.Infrastructure.WebAPI.Extensions.Setup.Process;
using System.Net;
using Microsoft.Extensions.Options;

namespace CCOF.Infrastructure.WebAPI.Services.D365WebAPI
{


    public class D365WebApiService : ID365WebApiService
    {
        private readonly ID365AuthenticationService _authenticationService;
        private readonly D365AuthSettings _d365AuthSettings;

        public D365WebApiService(IOptionsSnapshot<D365AuthSettings> d365AuthSettings, ID365AuthenticationService authenticationService)
        {
            _d365AuthSettings = d365AuthSettings.Value;
            _authenticationService = authenticationService ?? throw new ArgumentNullException(nameof(authenticationService));
        }

        public HttpResponseMessage SendRetrieveRequestAsync(string query, bool formatted = false, int maxPageSize = 200)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, query);
            request.Headers.Add("Prefer", "odata.maxpagesize=" + maxPageSize.ToString());
            if (formatted)
                request.Headers.Add("Prefer", "odata.include-annotations=OData.Community.Display.V1.FormattedValue");

            var client = _authenticationService.GetHttpClient().Result;

            return client.SendAsync(request).Result;
        }

        public HttpResponseMessage SendRetrieveRequestAsync1(AZAppUser spn, string query, bool formatted = false, int maxPageSize = 200)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, query);
            request.Headers.Add("Prefer", "odata.maxpagesize=" + maxPageSize.ToString());

            if (formatted)
                request.Headers.Add("Prefer", "odata.include-annotations=OData.Community.Display.V1.FormattedValue");

            HttpClient client = _authenticationService.GetHttpClientAsync(D365ServiceType.CRUD, spn).Result;

            return client.SendAsync(request).Result;
        }
        public HttpResponseMessage SendCreateRequestAsync(string endPoint, string content)
        {
            return SendAsync(HttpMethod.Post, endPoint, content);
        }

        public HttpResponseMessage SendCreateRequestAsyncRtn(string endPoint, string content)
        {
            return SendAsyncRtn(HttpMethod.Post, endPoint, content);
        }

        public HttpResponseMessage SendUpdateRequestAsync(string endPoint, string body)
        {
            var message = new HttpRequestMessage(HttpMethod.Patch, endPoint);
            message.Content = new StringContent(body, Encoding.UTF8, "application/json");

            var client = _authenticationService.GetHttpClient().Result;
            return client.SendAsync(message).Result;
        }

        public HttpResponseMessage SendDeleteRequestAsync(string endPoint)
        {
            return _authenticationService.GetHttpClient().Result.DeleteAsync(endPoint).Result;
        }

        public HttpResponseMessage SendCreateRequestAsync(HttpMethod httpMethod, string entitySetName, string body)
        {
            var message = new HttpRequestMessage(httpMethod, entitySetName);
            message.Content = new StringContent(body, Encoding.UTF8, "application/json");

            var client = _authenticationService.GetHttpClient().Result;

            return client.SendAsync(message).Result;
        }

        public HttpResponseMessage SendMessageAsync(HttpMethod httpMethod, string requestUri)
        {

            var client = _authenticationService.GetHttpClient().Result;
            HttpRequestMessage message = new(httpMethod, requestUri);

            return client.SendAsync(message).Result;
        }

        public HttpResponseMessage SendRetrieveAsync(AZAppUser spn, HttpMethod httpMethod, string requestUri)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUri);

            var client = _authenticationService.GetHttpClientAsync(D365ServiceType.CRUD, spn).Result;

            return client.SendAsync(request).Result;
            // var client = _authenticationService.GetHttpClient().Result;
            // HttpRequestMessage message = new(httpMethod, messageUri);

            // return client.SendAsync(message).Result;
        }

        public HttpResponseMessage SendSearchRequestAsync(string body)
        {
            var message = new HttpRequestMessage()
            {
                Content = new StringContent(body, Encoding.UTF8, "application/json")
            };
            message.Method = HttpMethod.Post;

            var client = _authenticationService.GetHttpClient(isSearch: true).Result;

            return client.SendAsync(message).Result;
        }

        private HttpResponseMessage SendAsync(HttpMethod operation, string endPoint, string body)
        {
            var message = new HttpRequestMessage(operation, endPoint);
            message.Content = new StringContent(body, Encoding.UTF8, "application/json");
            message.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

            return _authenticationService.GetHttpClient().Result.SendAsync(message).Result;
        }

        private HttpResponseMessage SendAsyncRtn(HttpMethod operation, string endPoint, string body)
        {
            var message = new HttpRequestMessage(operation, endPoint);
            message.Content = new StringContent(body, Encoding.UTF8, "application/json");
            message.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
            message.Headers.Add("Prefer", "return=representation");

            return _authenticationService.GetHttpClient().Result.SendAsync(message).Result;
        }


        public async Task<HttpResponseMessage> SendRetrieveRequestAsync(AZAppUser spn, string requestUri, bool formatted = false, int pageSize = 50, bool isProcess = false)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            if (!isProcess)
                request.Headers.Add("Prefer", "odata.maxpagesize=" + pageSize.ToString());

            if (formatted)
                request.Headers.Add("Prefer", "odata.include-annotations=OData.Community.Display.V1.FormattedValue");

            HttpClient client = await _authenticationService.GetHttpClientAsync(D365ServiceType.CRUD, spn);

            return await client.SendAsync(request);
        }

        public async Task<HttpResponseMessage> SendCreateRequestAsync(AZAppUser spn, string entitySetName, string requestBody)
        {
            HttpRequestMessage message = new(HttpMethod.Post, entitySetName)
            {
                Content = new StringContent(requestBody, Encoding.UTF8, "application/json")
            };
            message.Headers.Add("Prefer", "return=representation");

            HttpClient client = await _authenticationService.GetHttpClientAsync(D365ServiceType.CRUD, spn);

            return await client.SendAsync(message);
        }

        public async Task<HttpResponseMessage> SendPatchRequestAsync(AZAppUser spn, string requestUri, string requestBody)
        {
            HttpRequestMessage message = new(HttpMethod.Patch, requestUri)
            {
                Content = new StringContent(requestBody, Encoding.UTF8, "application/json")
            };
            message.Headers.Add("Prefer", "return=representation");

            HttpClient client = await _authenticationService.GetHttpClientAsync(D365ServiceType.CRUD, spn);

            return await client.SendAsync(message);
        }



        public async Task<HttpResponseMessage> SendDocumentRequestAsync(AZAppUser spn, string entityNameSet, Guid id, Byte[] data, string fileName)
        {
            UploadFileRequest request;
            if (entityNameSet.Equals("ofm_payment_file_exchanges"))
            {
                request = new(new D365EntityReference(entityNameSet, id), columnName: "ofm_input_document_memo", data, fileName);
            }
            else if (entityNameSet.Equals("ofm_allowances"))
            {
                request = new(new D365EntityReference(entityNameSet, id), columnName: "ofm_approval_pdf", data, fileName);
            }
            else
            {
                request = new(new D365EntityReference(entityNameSet, id), columnName: "ofm_data_file", data, fileName);
            }
            HttpClient client = await _authenticationService.GetHttpClientAsync(D365ServiceType.CRUD, spn);

            return await client.SendAsync(request);
        }

        public async Task<HttpResponseMessage> SendDeleteRequestAsync(AZAppUser spn, string requestUri)
        {
            HttpClient client = await _authenticationService.GetHttpClientAsync(D365ServiceType.CRUD, spn);

            return await client.DeleteAsync(requestUri);
        }


        public async Task<HttpResponseMessage> SendPostRequestAsync(AZAppUser spn, Guid newEmailId)
        {
            HttpClient client = await _authenticationService.GetHttpClientAsync(D365ServiceType.CRUD, spn);
            CCOF.Infrastructure.WebAPI.Messages.SendEmailRequest req;
            req = new(newEmailId, new JsonObject() {
                        { "IssueSend" , true} });

            var response = await client.PostAsync(req.RequestUri, req.Content);
            return response;
        }

        public async Task<HttpResponseMessage> SendSearchRequestAsync(AZAppUser spn, string body)
        {
            var message = new HttpRequestMessage()
            {
                Content = new StringContent(body, Encoding.UTF8, "application/json"),
                Method = HttpMethod.Post
            };

            HttpClient client = await _authenticationService.GetHttpClientAsync(D365ServiceType.Search, spn);

            return await client.SendAsync(message);
        }

        public async Task<BatchResult> SendBatchMessageAsync(AZAppUser spn, List<HttpRequestMessage> requestMessages, Guid? callerObjectId)
        {
            BatchRequest batchRequest = new(_d365AuthSettings)
            {
                Requests = requestMessages,
                ContinueOnError = true
            };
            if (callerObjectId != null && callerObjectId != Guid.Empty)
                batchRequest.Headers.Add("CallerObjectId", callerObjectId.ToString());

            HttpClient client = await _authenticationService.GetHttpClientAsync(D365ServiceType.Batch, spn);
            BatchResponse batchResponse = await SendAsync<BatchResponse>(batchRequest, client);

            Int16 processed = 0;
            List<string> errors = [];
            List<JsonObject> results = [];

            if (batchResponse.IsSuccessStatusCode)
                batchResponse.HttpResponseMessages.ForEach(async res =>
                {
                    if (res.IsSuccessStatusCode)
                    {
                        processed++;
                        if (res.StatusCode != HttpStatusCode.NoContent)
                        {
                            results.Add(await res.Content.ReadFromJsonAsync<JsonObject>());
                        }
                    }
                    else
                    {
                        errors.Add(await res.Content.ReadAsStringAsync());
                    }
                });

            if (errors.Any())
            {
                var batchResult = BatchResult.Failure(errors, 0, 0);

                if (errors.Count < requestMessages.Count)
                    batchResult = BatchResult.PartialSuccess(null, errors, processed, requestMessages.Count);

                //_logger.LogError(CustomLogEvent.Batch, "Batch operation finished with an error {error}", JsonValue.Create<BatchResult>(batchResult));

                return batchResult;
            }

            return BatchResult.Success(results, processed, requestMessages.Count); ;
        }
        
        #region Helpers

        public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, HttpClient client)
        {
            //// Session token used by elastic tables to enable strong consistency
            //// See https://learn.microsoft.com/power-apps/developer/data-platform/use-elastic-tables?tabs=webapi#sending-the-session-token
            //if (!string.IsNullOrWhiteSpace(_sessionToken) && request.Method == HttpMethod.Get)
            //{
            //    request.Headers.Add("MSCRM.SessionToken", _sessionToken);
            //}

            //HttpClient client = await _authenticationService.GetHttpClientAsync(D365ServiceType.Batch, spn);


            // Set the access token using the function from the Config passed to the constructor
            //request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", await config.GetAccessToken());

            // Get the named HttpClient from the IHttpClientFactory
            //var client = GetHttpClientFactory().CreateClient(WebAPIClientName);

            HttpResponseMessage response = await client.SendAsync(request);

            //// Capture the current session token value
            //// See https://learn.microsoft.com/power-apps/developer/data-platform/use-elastic-tables?tabs=webapi#getting-the-session-token
            //if (response.Headers.Contains("x-ms-session-token"))
            //{
            //    _sessionToken = response.Headers.GetValues("x-ms-session-token")?.FirstOrDefault()?.ToString();
            //}

            // Throw an exception if the request is not successful
            if (!response.IsSuccessStatusCode)
            {
                D365ServiceException exception = await ParseError(response);
                throw exception;
            }
            return response;
        }

        /// <summary>
        /// Processes requests with typed responses
        /// </summary>
        /// <typeparam name="T">The type derived from HttpResponseMessage</typeparam>
        /// <param name="request">The request</param>
        /// <param name="client"></param>
        /// <returns></returns>
        public async Task<T> SendAsync<T>(HttpRequestMessage request, HttpClient client) where T : HttpResponseMessage
        {
            HttpResponseMessage response = await SendAsync(request, client);

            // 'As' method is Extension of HttpResponseMessage see Extensions.cs
            return response.As<T>();
        }

        public async Task<D365ServiceException> ParseError(HttpResponseMessage response)
        {
            string requestId = string.Empty;
            if (response.Headers.Contains("REQ_ID"))
            {
                requestId = response.Headers.GetValues("REQ_ID").FirstOrDefault();
            }

            var content = await response.Content.ReadAsStringAsync();
            ODataError? oDataError = null;

            try
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                oDataError = JsonSerializer.Deserialize<ODataError>(content, options);
            }
            catch (Exception)
            {
                // Error may not be in correct OData Error format, so keep trying...
            }

            if (oDataError?.Error != null)
            {
                var exception = new D365ServiceException(oDataError.Error.Message)
                {
                    ODataError = oDataError,
                    Content = content,
                    ReasonPhrase = response.ReasonPhrase,
                    HttpStatusCode = response.StatusCode,
                    RequestId = requestId
                };
                return exception;
            }
            else
            {
                try
                {
                    ODataException oDataException = JsonSerializer.Deserialize<ODataException>(content);

                    D365ServiceException otherException = new(oDataException.Message)
                    {
                        Content = content,
                        ReasonPhrase = response.ReasonPhrase,
                        HttpStatusCode = response.StatusCode,
                        RequestId = requestId
                    };
                    return otherException;

                }
                catch (Exception)
                {

                }

                //When nothing else works
                D365ServiceException exception = new(response.ReasonPhrase)
                {
                    Content = content,
                    ReasonPhrase = response.ReasonPhrase,
                    HttpStatusCode = response.StatusCode,
                    RequestId = requestId
                };
                return exception;
            }
        }

        public async Task<HttpResponseMessage> GetRecordTemplateForClone(AZAppUser spn, Guid recordId, string targetEntityName, string targetEntityNameSet)
        {
            string entityMoniker = $$"""
                                {"@odata.id": "{{targetEntityNameSet}}({{recordId}})"}
                                """;
            var requestUri = $"""                                
                            InitializeFrom(EntityMoniker=@p1,TargetEntityName=@p2,TargetFieldType=@p3)?@p1={entityMoniker}&@p2='{targetEntityName}'&@p3=Microsoft.Dynamics.CRM.TargetFieldType'ValidForCreate'
                            """;

            HttpRequestMessage request = new(HttpMethod.Get, requestUri);

            HttpClient client = await _authenticationService.GetHttpClientAsync(D365ServiceType.CRUD, spn);

            return await client.SendAsync(request);
        }

        #endregion
    }
}