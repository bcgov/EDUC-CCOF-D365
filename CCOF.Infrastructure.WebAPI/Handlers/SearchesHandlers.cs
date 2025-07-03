using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using CCOF.Infrastructure.WebAPI.Extensions;
using CCOF.Infrastructure.WebAPI.Models;
using CCOF.Infrastructure.WebAPI.Services.AppUsers;
using CCOF.Infrastructure.WebAPI.Services.D365WebApi;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace CCOF.Infrastructure.WebAPI.Handlers;

public static class SearchesHandlers
{
    public static async Task<Results<BadRequest<string>, ProblemHttpResult, Ok<JsonObject>>> DataverseSearchAsync(
        ID365WebApiService d365WebApiService,
        ID365AppUserService appUserService,
        ILogger<string> logger,
        [FromBody] dynamic? searchTerm)
    {
       throw new NotImplementedException();
    }
}