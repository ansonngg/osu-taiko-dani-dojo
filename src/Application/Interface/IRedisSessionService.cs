using OsuTaikoDaniDojo.Application.Model;

namespace OsuTaikoDaniDojo.Application.Interface;

public interface IRedisSessionService
{
    Task SaveSessionAsync(string sessionId, object sessionData, int timeToLiveInSecond);
    Task<UserSession?> GetSessionAsync(string sessionId);
    Task DeleteSessionAsync(string sessionId);
    Task<bool> ExistsSessionAsync(string sessionId);
}
