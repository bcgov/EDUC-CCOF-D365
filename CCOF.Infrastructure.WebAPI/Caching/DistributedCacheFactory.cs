﻿namespace CCOF.Infrastructure.WebAPI.Caching;

public class DistributedCacheFactory : IDistributedCacheFactory
{
    private readonly IServiceProvider _serviceProvider;

    public DistributedCacheFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IDistributedCache<T> GetCache<T>() => _serviceProvider.GetRequiredService<IDistributedCache<T>>();
}