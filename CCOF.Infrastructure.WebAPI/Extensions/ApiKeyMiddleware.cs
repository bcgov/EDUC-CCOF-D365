﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using CCOF.Infrastructure.WebAPI.Models;
using System.Text.RegularExpressions;

namespace CCOF.Infrastructure.WebAPI.Extensions;

public class ApiKeyMiddleware(RequestDelegate next, ILoggerFactory loggerFactory)
{
    private readonly RequestDelegate _next = next;
    private readonly ILogger _logger = loggerFactory.CreateLogger(LogCategory.API);

    public async Task InvokeAsync(HttpContext context,
        IOptionsSnapshot<AuthenticationSettings> options)
    {
        if (context.Request.Method == "OPTIONS" || context.Request.Method == "TRACE" || context.Request.Method == "TRACK")
        {
            context.Response.StatusCode = StatusCodes.Status405MethodNotAllowed;
            _logger.LogError(CustomLogEvent.API, "Status405MethodNotAllowed: Attempted HTTP Verb: [{verb}]", context.Request.Method);

            return;
        }

        var apiKeys = options.Value.Schemes.ApiKeyScheme.Keys;
        var apiKeyPresentInHeader = context.Request.Headers.TryGetValue(options.Value.Schemes.ApiKeyScheme.ApiKeyName ?? "", out var extractedApiKey);

        if ((apiKeyPresentInHeader && apiKeys.Any(k => k.Value == extractedApiKey))
            || context.Request.Path.StartsWithSegments("/swagger"))
        {
            string newKeyValue = extractedApiKey.ToString();
            //var emailPattern = @"(?<=[\w]{1})[\w-\._\+%]*(?=[\w]{1}@)";
            var pattern = @"(?<=[\w]{5})[\w-\._\+%]*(?=[\w]{3})";
            var maskedKey = Regex.Replace(newKeyValue ?? "", pattern, m => new string('*', m.Length));

            _logger.LogInformation(CustomLogEvent.API, "x-ccof-apikey:{maskedKey}", maskedKey);

            await _next(context);

            return;
        }

        var endpoint = context.GetEndpoint();
        var isAllowAnonymous = endpoint?.Metadata.Any(x => x.GetType() == typeof(AllowAnonymousAttribute));
        if (isAllowAnonymous == true)
        {
            //_logger.LogWarning(CustomLogEvent.API, "Anonymous user detected.");
            await _next(context);
            return;
        }

        _logger.LogError(CustomLogEvent.API, "Attempted Key: [x-ccof-apikey:{extractedApiKey}]", string.IsNullOrEmpty(extractedApiKey) ? "NA" : extractedApiKey);

        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        await context.Response.WriteAsync(options.Value.Schemes.ApiKeyScheme.ApiKeyErrorMesssage);
    }
}

public static class ApiKeyMiddlewareExtension
{
    public static IApplicationBuilder UseApiKey(
        this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ApiKeyMiddleware>();
    }
}
