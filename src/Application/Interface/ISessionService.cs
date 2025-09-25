namespace OsuTaikoDaniDojo.Application.Interface;

public interface ISessionService
{
    Task SaveSessionAsync(string sessionId, object sessionData);
    Task<T?> GetSessionAsync<T>(string sessionId);
    Task DeleteSessionAsync(string sessionId);
    Task<bool> ExistsSessionAsync(string sessionId);
}
