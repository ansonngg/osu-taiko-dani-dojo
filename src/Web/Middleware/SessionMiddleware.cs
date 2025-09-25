using Microsoft.Extensions.Options;
using OsuTaikoDaniDojo.Application.Interface;
using OsuTaikoDaniDojo.Application.Options;
using OsuTaikoDaniDojo.Web.Context;
using OsuTaikoDaniDojo.Web.Utility;
using SessionOptions = OsuTaikoDaniDojo.Application.Options.SessionOptions;

namespace OsuTaikoDaniDojo.Web.Middleware;

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

    private async Task<(SessionContext?, bool)> _RetrieveSessionAsync(string sessionId)
    {
        var session = await _sessionService.GetSessionAsync<SessionContext>(sessionId);

        if (session == null)
        {
            return (null, false);
        }

        if (DateTime.UtcNow < session.ExpiresAt.AddSeconds(-_tokenExpiryBufferInSecond))
        {
            return (session, false);
        }

        var newTokenQuery = await _osuAuthService.RefreshTokenAsync(session.RefreshToken);

        if (newTokenQuery == null)
        {
            await _sessionService.DeleteSessionAsync(sessionId);
            return (null, false);
        }

        var newSession = new SessionContext
        {
            UserId = session.UserId,
            AccessToken = newTokenQuery.AccessToken,
            RefreshToken = newTokenQuery.RefreshToken,
            ExpiresAt = newTokenQuery.ExpiresAt
        };

        await _sessionService.SaveSessionAsync(sessionId, newSession);
        return (newSession, true);
    }
}
