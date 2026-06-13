using Microsoft.Extensions.Options;
using OsuDojo.Application.Context;
using OsuDojo.Application.Interface;
using OsuDojo.Application.Options;

namespace OsuDojo.Application.Service;

public class LoginService(CachedSessionService sessionService, IOptions<LoginSessionOptions> loginSessionOptions)
    : ILoginService
{
    private static readonly TimeSpan SessionListExpiryBuffer = TimeSpan.FromMinutes(1);
    private readonly CachedSessionService _sessionService = sessionService;
    private readonly TimeSpan _loginSessionExpiry = TimeSpan.FromDays(loginSessionOptions.Value.SessionExpiryInDay);
    private readonly int _maxSessionCountPerUser = loginSessionOptions.Value.MaxSessionCountPerUser;

    public async Task<string> CreateLoginSessionAsync(LoginSessionContext loginSessionContext)
    {
        var userId = loginSessionContext.UserId.ToString();
        var sessionListContext = await _sessionService.GetSessionAsync<SessionListContext>(userId);

        var newSessionListContext = sessionListContext != null
            ? await _UpdateSessionListAsync(sessionListContext, loginSessionContext)
            : new SessionListContext();

        var sessionId = await _GenerateUniqueSessionIdAsync();
        await _sessionService.SaveSessionAsync(sessionId, loginSessionContext, _loginSessionExpiry);
        newSessionListContext.SessionIds.Add(sessionId);

        await _sessionService.SaveSessionAsync(
            userId,
            newSessionListContext,
            _loginSessionExpiry + SessionListExpiryBuffer);

        return sessionId;
    }

    public async Task<LoginSessionContext?> GetLoginSessionAsync(string sessionId)
    {
        return await _sessionService.GetSessionAsync<LoginSessionContext>(sessionId);
    }

    public async Task UpdateLoginSessionAsync(string sessionId, LoginSessionContext loginSessionContext)
    {
        var userId = loginSessionContext.UserId.ToString();
        var sessionListContext = await _sessionService.GetSessionAsync<SessionListContext>(userId);
        SessionListContext newSessionListContext;

        if (sessionListContext != null)
        {
            sessionListContext.SessionIds.Remove(sessionId);
            newSessionListContext = await _UpdateSessionListAsync(sessionListContext, loginSessionContext);
        }
        else
        {
            newSessionListContext = new SessionListContext();
        }

        await _sessionService.SaveSessionAsync(sessionId, loginSessionContext, _loginSessionExpiry);
        newSessionListContext.SessionIds.Add(sessionId);

        await _sessionService.SaveSessionAsync(
            userId,
            newSessionListContext,
            _loginSessionExpiry + SessionListExpiryBuffer);
    }

    public async Task DeleteLoginSessionAsync(string sessionId)
    {
        await _sessionService.DeleteSessionAsync(sessionId);
    }

    private async Task<SessionListContext> _UpdateSessionListAsync(
        SessionListContext sessionListContext,
        LoginSessionContext loginSessionContext)
    {
        var newSessionListContext = new SessionListContext();

        for (var i = 0; i < sessionListContext.SessionIds.Count - (_maxSessionCountPerUser - 1); i++)
        {
            await _sessionService.DeleteSessionAsync(sessionListContext.SessionIds[i]);
        }

        for (var i = Math.Max(0, sessionListContext.SessionIds.Count - (_maxSessionCountPerUser - 1));
             i < sessionListContext.SessionIds.Count;
             i++)
        {
            if (!await _sessionService.ExistsSessionAsync(sessionListContext.SessionIds[i]))
            {
                continue;
            }

            await _sessionService.SaveSessionAsync(sessionListContext.SessionIds[i], loginSessionContext);
            newSessionListContext.SessionIds.Add(sessionListContext.SessionIds[i]);
        }

        return newSessionListContext;
    }

    private async Task<string> _GenerateUniqueSessionIdAsync()
    {
        var sessionId = Guid.NewGuid().ToString();

        while (await _sessionService.ExistsSessionAsync(sessionId))
        {
            sessionId = Guid.NewGuid().ToString();
        }

        return sessionId;
    }
}
