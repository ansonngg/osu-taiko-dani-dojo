using Application.Model;

namespace Application.Interface;

public interface IRedisSessionService
{
    Task SaveSessionAsync(string sessionId, object sessionData);
    Task<UserSession?> GetSessionAsync(string sessionId);
    Task DeleteSessionAsync(string sessionId);
    Task<bool> ExistsSessionAsync(string sessionId);
}
