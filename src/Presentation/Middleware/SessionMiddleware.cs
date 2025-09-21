using Microsoft.Extensions.Options;
using OsuTaikoDaniDojo.Application.Interface;
using OsuTaikoDaniDojo.Application.Model;
using OsuTaikoDaniDojo.Application.Options;
using OsuTaikoDaniDojo.Presentation.Utility;
using SessionOptions = OsuTaikoDaniDojo.Application.Options.SessionOptions;

namespace OsuTaikoDaniDojo.Presentation.Middleware;

public class SessionMiddleware(
    RequestDelegate next,
    IOsuAuthService osuAuthService,
    ISessionService sessionService,
    IOptions<SessionOptions> sessionOptions,
    IOptions<OsuOptions> osuOptions)
{
    private readonly RequestDelegate _next = next;
    private readonly IOsuAuthService _osuAuthService = osuAuthService;
    private readonly ISessionService _sessionService = sessionService;
    private readonly int _cookieSessionExpiryInDay = sessionOptions.Value.CookieExpiryInDay;
    private readonly int _tokenExpiryBufferInSecond = osuOptions.Value.TokenExpiryBufferInSecond;

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
                DateTimeOffset.UtcNow.AddDays(_cookieSessionExpiryInDay));
        }

        context.Items["UserSession"] = session;
    }

    private async Task<(UserSession?, bool)> _RetrieveSessionAsync(string sessionId)
    {
        var session = await _sessionService.GetSessionAsync(sessionId);

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
            await _sessionService.DeleteSessionAsync(sessionId);
            return (null, false);
        }

        var newSession = new UserSession { UserId = session.UserId, UserToken = newUserToken };
        await _sessionService.SaveSessionAsync(sessionId, newSession);
        return (newSession, true);
    }
}
