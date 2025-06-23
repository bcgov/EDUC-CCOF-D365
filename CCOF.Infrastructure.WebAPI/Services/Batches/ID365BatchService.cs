using CCOF.Infrastructure.WebAPI.Services.Processes;
using System.Text.Json;

namespace CCOF.Infrastructure.WebAPI.Services.Batches;

public interface ID365BatchService 
{
    Task<ProcessResult> ExecuteAsync(JsonDocument jsonDocument, Int16 batchTypeId);
}