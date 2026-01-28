using System;

namespace RentADad.Application.Abstractions.Caching;

public interface ICacheStore
{
    bool TryGet<T>(string key, out T? value);
    void Set<T>(string key, T value, TimeSpan ttl);
    void Remove(string key);
}
