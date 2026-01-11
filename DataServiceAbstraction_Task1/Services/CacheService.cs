using DataServiceAbstraction_Task1.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace DataServiceAbstraction_Task1.Services;
public class CacheService(IMemoryCache memoryCache,ILogger logger) : ICacheService
{
    private readonly IMemoryCache _memoryCache = memoryCache;
    private readonly ILogger _logger = logger;

    public object Get(string key) => _memoryCache.Get(key);

    public void Set(string key, object value,TimeSpan duration)
    {
        _logger.LogInformation($"Setting item in cache with key: {key} for duration: {duration}");

        var cacheOptions = new MemoryCacheEntryOptions()
            .SetSlidingExpiration(TimeSpan.FromMinutes(30))
            .SetAbsoluteExpiration(duration);

        _memoryCache.Set(key, value, cacheOptions);
    }

    public void Delete(string key)
    {
        _logger.LogInformation($"Removing item from cache with key: {key}");
        _memoryCache.Remove(key);
    }

}