using Microsoft.Extensions.Caching.Memory;
using OsuTaikoDaniDojo.Application.Interface;

namespace OsuTaikoDaniDojo.Infrastructure.Service;

public class HybridSessionService(
    RedisSessionService redisSessionService,
    IMemoryCache memoryCache)
    : ISessionService
{
    private static readonly TimeSpan MemoryCacheSessionExpiry = TimeSpan.FromMinutes(60);
    private readonly RedisSessionService _redisSessionService = redisSessionService;
    private readonly IMemoryCache _memoryCache = memoryCache;

    public async Task SaveSessionAsync(string sessionId, object sessionData, TimeSpan? timeToLive)
    {
        if (timeToLive != null)
        {
            _memoryCache.Set(sessionId, sessionData, MemoryCacheSessionExpiry);
        }

        await _redisSessionService.SaveSessionAsync(sessionId, sessionData, timeToLive);
    }

    public async Task<T?> GetSessionAsync<T>(string sessionId)
    {
        if (_memoryCache.TryGetValue(sessionId, out T? session))
        {
            return session;
        }

        var redisSession = await _redisSessionService.GetSessionAsync<T>(sessionId);

        if (redisSession != null)
        {
            _memoryCache.Set(sessionId, redisSession, MemoryCacheSessionExpiry);
        }

        return redisSession;
    }

    public async Task DeleteSessionAsync(string sessionId)
    {
        _memoryCache.Remove(sessionId);
        await _redisSessionService.DeleteSessionAsync(sessionId);
    }

    public async Task<bool> ExistsSessionAsync(string sessionId)
    {
        return _memoryCache.TryGetValue(sessionId, out _) || await _redisSessionService.ExistsSessionAsync(sessionId);
    }
}
