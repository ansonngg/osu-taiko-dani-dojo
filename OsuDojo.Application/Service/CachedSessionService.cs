using Microsoft.Extensions.Caching.Memory;
using OsuDojo.Application.Interface;

namespace OsuDojo.Application.Service;

public class CachedSessionService(ISessionService sessionService, IMemoryCache memoryCache) : ISessionService
{
    private static readonly TimeSpan MemoryCacheSessionExpiry = TimeSpan.FromMinutes(60);
    private readonly ISessionService _sessionService = sessionService;
    private readonly IMemoryCache _memoryCache = memoryCache;

    public async Task SaveSessionAsync(string sessionId, object sessionData, TimeSpan? timeToLive = null)
    {
        if (timeToLive != null)
        {
            _memoryCache.Set(sessionId, sessionData, MemoryCacheSessionExpiry);
        }

        await _sessionService.SaveSessionAsync(sessionId, sessionData, timeToLive);
    }

    public async Task<T?> GetSessionAsync<T>(string sessionId)
    {
        if (_memoryCache.TryGetValue(sessionId, out T? session))
        {
            return session;
        }

        var redisSession = await _sessionService.GetSessionAsync<T>(sessionId);

        if (redisSession != null)
        {
            _memoryCache.Set(sessionId, redisSession, MemoryCacheSessionExpiry);
        }

        return redisSession;
    }

    public async Task DeleteSessionAsync(string sessionId)
    {
        _memoryCache.Remove(sessionId);
        await _sessionService.DeleteSessionAsync(sessionId);
    }

    public async Task<bool> ExistsSessionAsync(string sessionId)
    {
        return _memoryCache.TryGetValue(sessionId, out _) || await _sessionService.ExistsSessionAsync(sessionId);
    }
}
