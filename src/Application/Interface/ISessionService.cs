using OsuTaikoDaniDojo.Application.Model;

namespace OsuTaikoDaniDojo.Application.Interface;

public interface ISessionService
{
    Task SaveSessionAsync(string sessionId, object sessionData);
    Task<UserSession?> GetSessionAsync(string sessionId);
    Task DeleteSessionAsync(string sessionId);
    Task<bool> ExistsSessionAsync(string sessionId);
}
