﻿using CCOF.Infrastructure.WebAPI.Services.Processes;
using Microsoft.Extensions.Logging;
using System.Reflection.Metadata;

namespace CCOF.Infrastructure.WebAPI.Extensions;

public interface ID365BackgroundProcessHandler
{
    void Execute(Func<ID365ScheduledProcessService, Task> processor);
}

public class D365BackgroundProcessHandler : ID365BackgroundProcessHandler
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger _logger;

    //[FromServices] IServiceScopeFactory
    public D365BackgroundProcessHandler(IServiceScopeFactory serviceScopeFactory, ILoggerFactory loggerFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = loggerFactory.CreateLogger(LogCategory.Process);
    }

    public void Execute(Func<ID365ScheduledProcessService, Task> processor)
    {
        Task.Run(async () =>
        {
            try
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var service = scope.ServiceProvider.GetRequiredService<ID365ScheduledProcessService>();
                await processor(service);
            }
            catch (Exception exp)
            {
                _logger.LogCritical(exp.StackTrace);
                //_logger.LogCritical(exp.Message);
            }
        });
    }
}