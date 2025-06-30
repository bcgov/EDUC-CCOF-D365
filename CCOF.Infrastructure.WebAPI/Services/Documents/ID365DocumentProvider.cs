using CCOF.Infrastructure.WebAPI.Models;
using CCOF.Infrastructure.WebAPI.Services.AppUsers;
using CCOF.Infrastructure.WebAPI.Services.D365WebApi;
using System.Text.Json.Nodes;

namespace CCOF.Infrastructure.WebAPI.Services.Documents;

public interface ID365DocumentProvider
{
    string EntityNameSet { get; } //e.g. annotations etc.
    Task<string> PrepareDocumentBodyAsync(JsonObject jsonData, ID365AppUserService appUserService, ID365WebApiService d365WebApiService);
    Task<JsonObject> CreateDocumentAsync(FileMapping document, ID365AppUserService appUserService, ID365WebApiService d365WebApiService);
}