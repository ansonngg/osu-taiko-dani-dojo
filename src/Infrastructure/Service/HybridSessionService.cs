using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using OsuTaikoDaniDojo.Application.Interface;
using OsuTaikoDaniDojo.Application.Options;

namespace OsuTaikoDaniDojo.Infrastructure.Service;

public class HybridSessionService(
    RedisSessionService redisSessionService,
    IOptions<SessionOptions> sessionOptions,
    IMemoryCache memoryCache)
    : ISessionService
{
    private readonly RedisSessionService _redisSessionService = redisSessionService;
    private readonly IMemoryCache _memoryCache = memoryCache;

    private readonly TimeSpan _memoryCacheSessionExpiry
        = TimeSpan.FromMinutes(sessionOptions.Value.MemoryCacheExpiryInMinute);

    public async Task SaveSessionAsync(string sessionId, object sessionData)
    {
        _memoryCache.Set(sessionId, sessionData, _memoryCacheSessionExpiry);
        await _redisSessionService.SaveSessionAsync(sessionId, sessionData);
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
            _memoryCache.Set(sessionId, redisSession, _memoryCacheSessionExpiry);
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
