﻿using CCOF.Infrastructure.WebAPI.Messages;
using CCOF.Infrastructure.WebAPI.Services.AppUsers;
using CCOF.Infrastructure.WebAPI.Services.Batches;
using CCOF.Infrastructure.WebAPI.Services.D365WebApi;
using CCOF.Infrastructure.WebAPI.Services.Processes;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace CCOF.Infrastructure.WebAPI.Services.Documents;

public class BatchProvider : ID365BatchProvider
{
    public Int16 BatchTypeId { get => 100; }

    public async Task<JsonObject> PrepareDataAsync(JsonDocument jsonDocument, ID365AppUserService appUserService, ID365WebApiService d365WebApiService)
    {
        throw new NotImplementedException();
    }

    public async Task<JsonObject> ExecuteAsync(JsonDocument document, ID365AppUserService appUserService, ID365WebApiService d365WebApiService)
    {
        JsonElement root = document.RootElement;

        JsonElement data = root.GetProperty("data");
        List<HttpRequestMessage> requests = [];
        foreach (var jsonElement in data.EnumerateObject())
        {
            JsonObject jsonObject = [];
            if (jsonElement.Name != "" && jsonElement.Value.ValueKind != JsonValueKind.Null && jsonElement.Value.ValueKind != JsonValueKind.Undefined)
            {
                var obj = jsonElement.Value;
                if (jsonElement.Value.ValueKind == JsonValueKind.Object)
                {
                    var jsonRequest = await ProcessObjectData(obj, jsonObject);
                    requests.Add(jsonRequest);
                }
                else if (jsonElement.Value.ValueKind == JsonValueKind.Array)
                {
                    foreach (var load in obj.EnumerateArray())
                    {
                        jsonObject = [];
                        var req = await ProcessObjectData(load, jsonObject);
                        requests.Add(req);
                    }
                }
            }
        }

        var batchResult = await d365WebApiService.SendBatchMessageAsync(appUserService.AZPortalAppUser, requests, null);

        if (batchResult.Errors.Any())
        {
            var sendBatchError = ProcessResult.Failure(batchResult.ProcessId, batchResult.Errors, batchResult.TotalProcessed, batchResult.TotalRecords);
            return sendBatchError.SimpleProcessResult;
        }
        var result= ProcessResult.Success(batchResult.ProcessId, batchResult.TotalRecords);
        //return await Task.FromResult<JsonObject>(new JsonObject());
        return result.SimpleProcessResult;
    }

    private async Task<HttpRequestMessage> ProcessObjectData(JsonElement payload, JsonObject keyValuePairs)
    {
        foreach (var data in payload.EnumerateObject())
        {
            keyValuePairs.Add(data.Name, (JsonNode)data.Value.ToString());
        }
        var entityName = keyValuePairs["entityNameSet"];
        var entityId = keyValuePairs["entityID"].ToString();
        keyValuePairs.Remove("entityNameSet");
        keyValuePairs.Remove("entityID");
        keyValuePairs.Remove("actionMode");
        var request = new D365UpdateRequest(new D365EntityReference(entityName.ToString(), new Guid(entityId)), keyValuePairs);
        return request;
    }
}