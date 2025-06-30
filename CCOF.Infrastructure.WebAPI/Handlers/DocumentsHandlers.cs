﻿using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using CCOF.Infrastructure.WebAPI.Extensions;
using CCOF.Infrastructure.WebAPI.Models;
using System.Text.Json;
using System.Text.Json.Nodes;
using CCOF.Infrastructure.WebAPI.Services.Documents;
using CCOF.Infrastructure.WebAPI.Services.Processes;

namespace CCOF.Infrastructure.WebAPI.Handlers;
public static class DocumentsHandlers
{
    /// <summary>
    /// ToDo
    /// </summary>
    /// <returns></returns>
    public static async Task<Results<BadRequest<string>, ProblemHttpResult, Ok<JsonObject>>> GetAsync(
        ID365DocumentService documentService,
        string documentId)
    {
        throw new NotImplementedException();

        //if (string.IsNullOrEmpty(documentId)) return TypedResults.BadRequest("Invalid Query.");

        //var response = await documentService.GetAsync(documentId);

        //if (!response.IsSuccessStatusCode)
        //{
        //    return TypedResults.Problem($"Failed to Retrieve record: {response.ReasonPhrase}", statusCode: (int)response.StatusCode);
        //}

        //var result = await response.Content.ReadFromJsonAsync<JsonObject>();
        //return TypedResults.Ok(result);
    }

    /// <summary>
    /// Upload multiple documents "<see cref="FileMapping"/>".
    /// </summary>
    /// <response code="201">Returns a list with the uploaded documentIds.</response>
    /// <param name="documentService"></param>
    /// <param name="loggerFactory"></param>
    /// <param name="files"></param>
    /// <param name="fileMapping" example=""></param>
    /// <returns></returns>
    /// <remarks>
    /// Sample request:
    ///
    /// POST api/documents
    ///
    ///  [
    ///    {
    ///       "ofm_subject": "income.jpg",
    ///       "ofm_extension": ".jpg",
    ///       "ofm_file_size": 95.5,
    ///       "ofm_description": "description 01",
    ///       "entity_name_set": "ofm_assistance_requests",
    ///       "regardingid": "00000000-0000-0000-0000-000000000000",
    ///       "ofm_category":"Income Statement"
    ///    },
    ///    {
    ///       "ofm_subject": "balancesheet.png",
    ///       "ofm_extension": ".png",
    ///       "ofm_file_size": 1000.5,
    ///       "ofm_description": "description 02",
    ///       "entity_name_set": "ofm_assistance_requests",
    ///       "regardingid": "00000000-0000-0000-0000-000000000000",
    ///       "ofm_category":"Balance Sheet"
    ///    }
    ///  ]
    /// </remarks>
    public static async Task<Results<ProblemHttpResult, BadRequest<string>, Ok<ProcessResult>>> PostAsync(
        ID365DocumentService documentService,
        ILoggerFactory loggerFactory,
        IFormFileCollection files,
        [FromForm] string fileMapping)
    {
        var logger = loggerFactory.CreateLogger(LogCategory.Process);
        using (logger.BeginScope("ScopeDocument"))
        {
            if (fileMapping.Length == 0 || !files.Any()) { return TypedResults.BadRequest("Invalid Query."); }

            var mappings = JsonSerializer.Deserialize<List<FileMapping>>(fileMapping)?.ToList();
            if (mappings is null || !mappings.Any()) return TypedResults.BadRequest("The fileMapping is not valid.");
     
            var hasUniqueNameIssue = mappings.GroupBy(f => f.ofm_subject).Any(c => c.Count() != 1);
            if (hasUniqueNameIssue) return TypedResults.BadRequest("Each filename must be unique.");

            var uploadResult = await documentService.UploadAsync(files, mappings!);

            if (uploadResult.Errors.Any()) {
                logger.LogError(CustomLogEvent.Document, "API Failure: Failed to upload documents. [Errors: {errors}]. [FileMapping:{fileMapping}]",uploadResult.Errors.ToArray(), JsonValue.Create(fileMapping).ToString());

                return TypedResults.Problem(uploadResult.SimpleProcessResult.ToJsonString());
            }

            return TypedResults.Ok(uploadResult);
        }
    }

    public static async Task<Results<ProblemHttpResult, Ok<string>>> DeleteAsync(
          ID365DocumentService documentService,
          string documentId)
    {
        var response = await documentService.RemoveAsync(documentId);

        if (response.IsSuccessStatusCode)
            return TypedResults.Ok($"Record was removed successfully.");
        else
            return TypedResults.Problem($"Failed to Delete record: {response.ReasonPhrase}", statusCode: (int)response.StatusCode);
    }
}