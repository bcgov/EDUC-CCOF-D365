﻿namespace CCOF.Infrastructure.WebAPI.Caching;

public interface IDistributedCache<T>
{
    T? Get(string key);
    Task<T?> GetAsync(string key);
    Task RemoveAsync(string key);
    Task SetAsync(string key, T item, int minutesToCache);
    Task<(bool Found, T? Value)> TryGetValueAsync(string key);
}
