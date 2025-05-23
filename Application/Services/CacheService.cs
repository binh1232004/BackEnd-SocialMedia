using Application.Interfaces.ServiceInterfaces;
using Microsoft.Extensions.Caching.Memory;

namespace Application.Services;

public class CacheService : ICacheService
{
    private readonly IMemoryCache _memoryCache;

    public CacheService(IMemoryCache memoryCache)
    {
        _memoryCache = memoryCache;
    }

    public async Task SetUserOnlineAsync(string userId)
    {
        await Task.Run(() =>
        {
            _memoryCache.Set($"user:{userId}:online", true, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30)
            });
        });
    }

    public async Task SetUserOfflineAsync(string userId)
    {
        await Task.Run(() => { _memoryCache.Remove($"user:{userId}:online"); });
    }

    public async Task<bool> IsUserOnlineAsync(string userId)
    {
        return await Task.Run(() =>
        {
            return _memoryCache.TryGetValue($"user:{userId}:online", out bool isOnline) && isOnline;
        });
    }
}