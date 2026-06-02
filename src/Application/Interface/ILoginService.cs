using OsuTaikoDaniDojo.Application.Context;

namespace OsuTaikoDaniDojo.Application.Interface;

public interface ILoginService
{
    Task<string> CreateLoginSessionAsync(LoginSessionContext loginSessionContext);
    Task<LoginSessionContext?> GetLoginSessionAsync(string sessionId);
    Task UpdateLoginSessionAsync(string sessionId, LoginSessionContext loginSessionContext);
    Task DeleteLoginSessionAsync(string sessionId);
}
