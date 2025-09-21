using Microsoft.Extensions.Options;
using OsuTaikoDaniDojo.Application.Interface;
using OsuTaikoDaniDojo.Application.Model;
using OsuTaikoDaniDojo.Application.Options;
using OsuTaikoDaniDojo.Presentation.Utility;

namespace OsuTaikoDaniDojo.Presentation.Middleware;

public class SessionMiddleware(
    RequestDelegate next,
    IOsuAuthService osuAuthService,
    IRedisSessionService redisSessionService,
    IOptions<OsuOptions> options)
{
    private readonly RequestDelegate _next = next;
    private readonly IOsuAuthService _osuAuthService = osuAuthService;
    private readonly IRedisSessionService _redisSessionService = redisSessionService;
    private readonly int _tokenExpiryBufferInSecond = options.Value.TokenExpiryBufferInSecond;

    public async Task InvokeAsync(HttpContext context)
    {
        var sessionId = context.Request.Cookies[ClientConst.SessionIdCookieName];

        if (!string.IsNullOrEmpty(sessionId))
        {
            await _ManageSessionAsync(context, sessionId);
        }

        await _next(context);
    }

    private async Task _ManageSessionAsync(HttpContext context, string sessionId)
    {
        var (session, isRefreshed) = await _RetrieveSessionAsync(sessionId);

        if (session == null)
        {
            context.Response.Cookies.Delete(ClientConst.SessionIdCookieName);
            return;
        }

        if (isRefreshed)
        {
            context.Response.Cookies.Append(
                ClientConst.SessionIdCookieName,
                sessionId,
                DateTimeOffset.UtcNow.AddSeconds(ClientConst.SessionIdCookieExpiryInSecond));
        }

        context.Items["UserSession"] = session;
    }

    private async Task<(UserSession?, bool)> _RetrieveSessionAsync(string sessionId)
    {
        var session = await _redisSessionService.GetSessionAsync(sessionId);

        if (session == null)
        {
            return (null, false);
        }

        if (DateTime.UtcNow < session.UserToken.ExpiresAt.AddSeconds(-_tokenExpiryBufferInSecond))
        {
            return (session, false);
        }

        var newUserToken = await _osuAuthService.RefreshTokenAsync(session.UserToken.RefreshToken);

        if (newUserToken == null)
        {
            await _redisSessionService.DeleteSessionAsync(sessionId);
            return (null, false);
        }

        var newSession = new UserSession { UserId = session.UserId, UserToken = newUserToken };
        await _redisSessionService.SaveSessionAsync(sessionId, newSession, ClientConst.SessionIdCookieExpiryInSecond);
        return (newSession, true);
    }
}
