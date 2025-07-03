using CCOF.Infrastructure.WebAPI.Models;
using CCOF.Infrastructure.WebAPI.Services.AppUsers;
using CCOF.Infrastructure.WebAPI.Services.D365WebApi;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace CCOF.Infrastructure.WebAPI.Services.Batches;

public interface ID365BatchProvider
{
    /// <summary>
    /// e.g. 1
    /// </summary>
    short BatchTypeId { get; } 

    Task<JsonObject> PrepareDataAsync(JsonDocument jsonDocument, ID365AppUserService appUserService, ID365WebApiService d365WebApiService);

    Task<JsonObject> ExecuteAsync(JsonDocument jsonDocument, ID365AppUserService appUserService, ID365WebApiService d365WebApiService);
}