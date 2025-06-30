using CCOF.Infrastructure.WebAPI.Services.AppUsers;
using CCOF.Infrastructure.WebAPI.Services.D365WebApi;
using System.Text.Json.Nodes;

namespace CCOF.Infrastructure.WebAPI.Services.Processes;

public interface ID365ProcessProvider
{
    short ProcessId { get; }
    string ProcessName { get; }
    Task<ProcessData> GetDataAsync();
    Task<JsonObject> RunProcessAsync(ID365AppUserService appUserService, ID365WebApiService d365WebApiService, ProcessParameter processParams);
}

public interface ID365OnDemandProcessProvider
{
    short ProcessId { get; }
    string ProcessName { get; }
    Task<ProcessData> GetDataAsync();
    Task<ProcessResult> RunOnDemandProcessAsync(ID365AppUserService appUserService, ID365WebApiService d365WebApiService, ProcessParameter processParams);
}