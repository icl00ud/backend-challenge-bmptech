using Microsoft.Extensions.Caching.Distributed;
using ChuBank.Domain.Interfaces;
using System.Text.Json;

namespace ChuBank.Infrastructure.Services;

public class CacheService : ICacheService
{
    private readonly IDistributedCache _distributedCache;

    public CacheService(IDistributedCache distributedCache)
    {
        _distributedCache = distributedCache;
    }

    public async Task<T?> GetAsync<T>(string key) where T : class
    {
        var value = await _distributedCache.GetStringAsync(key);
        
        if (string.IsNullOrEmpty(value))
            return null;

        return JsonSerializer.Deserialize<T>(value);
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null) where T : class
    {
        var serializedValue = JsonSerializer.Serialize(value);
        
        var options = new DistributedCacheEntryOptions();
        
        if (expiry.HasValue)
            options.SetAbsoluteExpiration(expiry.Value);
        else
            options.SetAbsoluteExpiration(TimeSpan.FromMinutes(30)); // Default 30 minutes

        await _distributedCache.SetStringAsync(key, serializedValue, options);
    }

    public async Task RemoveAsync(string key)
    {
        await _distributedCache.RemoveAsync(key);
    }
}
