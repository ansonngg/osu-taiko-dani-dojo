using Application.Interface;
using Application.Model;
using OsuTaikoDaniDojo.Presentation.Utility;

namespace OsuTaikoDaniDojo.Presentation.Middleware;

public class SessionMiddleware(
    RequestDelegate next,
    IOsuAuthService osuAuthService,
    IRedisSessionService redisSessionService)
{
    private const float ExpiryBufferInSecond = 60;

    private readonly RequestDelegate _next = next;
    private readonly IOsuAuthService _osuAuthService = osuAuthService;
    private readonly IRedisSessionService _redisSessionService = redisSessionService;

    public async Task InvokeAsync(HttpContext context)
    {
        var sessionId = context.Request.Cookies[ClientConst.SessionIdCookies];

        if (!string.IsNullOrEmpty(sessionId))
        {
            var session = await _RetrieveSession(sessionId);

            if (session != null)
            {
                context.Items["UserSession"] = session;
            }
            else
            {
                context.Response.Cookies.Delete(ClientConst.SessionIdCookies);
            }
        }

        await _next(context);
    }

    private async Task<UserSession?> _RetrieveSession(string sessionId)
    {
        var session = await _redisSessionService.GetSessionAsync(sessionId);

        if (session == null)
        {
            return null;
        }

        if (DateTime.UtcNow < session.UserToken.ExpiresAt.AddSeconds(-ExpiryBufferInSecond))
        {
            return session;
        }

        var newUserToken = await _osuAuthService.RefreshTokenAsync(session.UserToken.RefreshToken);

        if (newUserToken == null)
        {
            await _redisSessionService.DeleteSessionAsync(sessionId);
            return null;
        }

        var newSession = new UserSession { UserId = session.UserId, UserToken = newUserToken };
        await _redisSessionService.SaveSessionAsync(sessionId, newSession);
        return newSession;
    }
}
