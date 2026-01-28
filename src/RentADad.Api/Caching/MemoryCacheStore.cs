using System;
using Microsoft.Extensions.Caching.Memory;
using RentADad.Application.Abstractions.Caching;

namespace RentADad.Api.Caching;

public sealed class MemoryCacheStore : ICacheStore
{
    private readonly IMemoryCache _cache;

    public MemoryCacheStore(IMemoryCache cache)
    {
        _cache = cache;
    }

    public bool TryGet<T>(string key, out T? value)
    {
        if (_cache.TryGetValue(key, out var cached) && cached is T typed)
        {
            value = typed;
            return true;
        }

        value = default;
        return false;
    }

    public void Set<T>(string key, T value, TimeSpan ttl)
    {
        _cache.Set(key, value, ttl);
    }

    public void Remove(string key)
    {
        _cache.Remove(key);
    }
}
