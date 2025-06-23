using CCOF.Infrastructure.WebAPI.Extensions;
using CCOF.Infrastructure.WebAPI.Messages;
using CCOF.Infrastructure.WebAPI.Models;
using CCOF.Infrastructure.WebAPI.Services.Processes;
using System.Text.Json.Nodes;

namespace CCOF.Infrastructure.WebAPI.Services.D365WebApi;

public interface ID365WebApiService
{
    Task<HttpResponseMessage> SendRetrieveRequestAsync(AZAppUser spn, string requestUrl, bool formatted = false, int pageSize = 50, bool isProcess = false);
    Task<HttpResponseMessage> SendCreateRequestAsync(AZAppUser spn, string entitySetName, string requestBody);
    Task<HttpResponseMessage> SendPatchRequestAsync(AZAppUser spn, string requestUrl, string content);
    Task<HttpResponseMessage> SendDeleteRequestAsync(AZAppUser spn, string requestUrl);
    Task<HttpResponseMessage> SendSearchRequestAsync(AZAppUser spn, string requestBody);
    Task<BatchResult> SendBatchMessageAsync(AZAppUser spn, List<HttpRequestMessage> requestMessages, Guid? callerObjectId);
    Task<HttpResponseMessage> SendDocumentRequestAsync(AZAppUser spn, string entityNameSet, Guid id, Byte[] data, string fileName);
    Task<HttpResponseMessage> SendBulkEmailTemplateMessageAsync(AZAppUser spn, JsonObject requestMessage, Guid? callerObjectId);
    Task<D365ServiceException> ParseError(HttpResponseMessage response);
    Task<HttpResponseMessage> GetRecordTemplateForClone(AZAppUser spn, Guid recordId, string targetEntityName, string targetEntityNameSet);
}