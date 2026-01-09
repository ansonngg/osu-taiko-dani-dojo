using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using OsuTaikoDaniDojo.Application.Interface;
using OsuTaikoDaniDojo.Application.Options;
using OsuTaikoDaniDojo.Web.Const;
using OsuTaikoDaniDojo.Web.Context;
using OsuTaikoDaniDojo.Web.Utility;
using SessionOptions = OsuTaikoDaniDojo.Application.Options.SessionOptions;

namespace OsuTaikoDaniDojo.Web.Handler;

public class SessionAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    IOsuAuthService osuAuthService,
    ISessionService sessionService,
    IUserRepository userRepository,
    IOptions<OsuOptions> osuOptions,
    IOptions<SessionOptions> sessionOptions)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    private readonly IOsuAuthService _osuAuthService = osuAuthService;
    private readonly ISessionService _sessionService = sessionService;
    private readonly IUserRepository _userRepository = userRepository;
    private readonly int _tokenExpiryBufferInMinute = osuOptions.Value.TokenExpiryBufferInMinute;
    private readonly int _cookieSessionExpiryInDay = sessionOptions.Value.CookieExpiryInDay;

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var sessionId = Request.Cookies[ClientConst.SessionIdCookieName];

        if (string.IsNullOrEmpty(sessionId))
        {
            return AuthenticateResult.NoResult();
        }

        var session = await _sessionService.GetSessionAsync<SessionContext>(sessionId);

        if (session == null)
        {
            Response.Cookies.Delete(ClientConst.SessionIdCookieName);
            return AuthenticateResult.Fail("Invalid session.");
        }

        if (DateTime.UtcNow >= session.ExpiresAt.AddMinutes(-_tokenExpiryBufferInMinute))
        {
            var newTokenQuery = await _osuAuthService.RefreshTokenAsync(session.RefreshToken);

            if (newTokenQuery == null)
            {
                await _sessionService.DeleteSessionAsync(sessionId);
                Response.Cookies.Delete(ClientConst.SessionIdCookieName);
                return AuthenticateResult.Fail("Invalid session.");
            }

            session = new SessionContext
            {
                UserId = session.UserId,
                OsuId = session.OsuId,
                Role = (await _userRepository.GetUserRoleAsync(session.OsuId))?.Role ?? "User",
                AccessToken = newTokenQuery.AccessToken,
                RefreshToken = newTokenQuery.RefreshToken,
                ExpiresAt = newTokenQuery.ExpiresAt
            };

            await _sessionService.SaveSessionAsync(sessionId, session);

            Response.Cookies.Append(
                ClientConst.SessionIdCookieName,
                sessionId,
                DateTimeOffset.UtcNow.AddDays(_cookieSessionExpiryInDay));
        }

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, session.UserId.ToString()),
            new Claim(ClaimTypes.Role, session.Role),
            new Claim(CustomClaimTypes.OsuId, session.OsuId.ToString()),
            new Claim(CustomClaimTypes.AccessToken, session.AccessToken)
        };

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        return AuthenticateResult.Success(new AuthenticationTicket(principal, Scheme.Name));
    }
}
