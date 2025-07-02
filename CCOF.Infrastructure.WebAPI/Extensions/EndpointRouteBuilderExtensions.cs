using CCOF.Infrastructure.WebAPI.Handlers;
using CCOF.Infrastructure.WebAPI.Handlers.D365;

namespace CCOF.Infrastructure.WebAPI.Extensions;

public static class EndpointRouteBuilderExtensions
{
    #region Portal

    public static void RegisterEnvironmentEndpoints(this IEndpointRouteBuilder endpointRouteBuilder)
    {
        var endpoints = endpointRouteBuilder.MapGroup("/api/environment");

        endpoints.MapGet("", EnvironmentHandlers.Get).WithTags("Portal Environment").Produces(200).ProducesProblem(404);
    }

    public static void RegisterSearchesEndpoints(this IEndpointRouteBuilder endpointRouteBuilder)
    {
        var searchesEndpoints = endpointRouteBuilder.MapGroup("/api/searches");

        searchesEndpoints.MapPost("", SearchesHandlers.DataverseSearchAsync).WithTags("Portal Searches").Produces(200).ProducesProblem(404);
    }

  

    public static void RegisterOperationsEndpoints(this IEndpointRouteBuilder endpointRouteBuilder)
    {
        var operationsEndpoints = endpointRouteBuilder.MapGroup("/api/operations");

        operationsEndpoints.MapGet("", OperationsHandlers.GetAsync).WithTags("Portal Operations").Produces(200).ProducesProblem(404);
        operationsEndpoints.MapPost("", OperationsHandlers.PostAsync).WithTags("Portal Operations").Produces(200).ProducesProblem(404);
        operationsEndpoints.MapPatch("", OperationsHandlers.PatchAsync).WithTags("Portal Operations").Produces(200).ProducesProblem(404);
        operationsEndpoints.MapDelete("", OperationsHandlers.DeleteAsync).WithTags("Portal Operations").Produces(200).ProducesProblem(404);
    }

    public static void RegisterBatchOperationsEndpoints(this IEndpointRouteBuilder endpointRouteBuilder)
    {
        var batchEndpoints = endpointRouteBuilder.MapGroup("/api/batches");

        batchEndpoints.MapPost("", BatchOperationsHandlers.BatchOperationsAsync).WithTags("Portal Batches").Produces(200).ProducesProblem(404);
    }

    #endregion

    #region D365

    public static void RegisterBatchProcessesEndpoints(this IEndpointRouteBuilder endpointRouteBuilder)
    {
        var requestsEndpoints = endpointRouteBuilder.MapGroup("/api/processes");

        requestsEndpoints.MapPost("/{processId}", ProcessesHandlers.RunProcessById).WithTags("D365 Processes");

    }

    #endregion
}