using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using OsuTaikoDaniDojo.Application.Interface;
using OsuTaikoDaniDojo.Application.Options;
using OsuTaikoDaniDojo.Web.Const;
using OsuTaikoDaniDojo.Web.Context;
using OsuTaikoDaniDojo.Web.Utility;

namespace OsuTaikoDaniDojo.Web.Handler;

public class SessionAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    IOsuAuthService osuAuthService,
    ISessionService sessionService,
    IUserRepository userRepository,
    IOptions<OsuOptions> osuOptions,
    IOptions<LoginSessionOptions> loginSessionOptions)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    private readonly IOsuAuthService _osuAuthService = osuAuthService;
    private readonly ISessionService _sessionService = sessionService;
    private readonly IUserRepository _userRepository = userRepository;
    private readonly int _tokenExpiryBufferInMinute = osuOptions.Value.TokenExpiryBufferInMinute;
    private readonly int _cookieSessionExpiryInDay = loginSessionOptions.Value.CookieExpiryInDay;

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var sessionId = Request.Cookies[AppDefaults.SessionIdCookieName];

        if (string.IsNullOrEmpty(sessionId))
        {
            return AuthenticateResult.NoResult();
        }

        var loginSessionContext = await _sessionService.GetSessionAsync<LoginSessionContext>(sessionId);

        if (loginSessionContext == null)
        {
            Response.Cookies.Delete(AppDefaults.SessionIdCookieName);
            return AuthenticateResult.NoResult();
        }

        if (DateTime.UtcNow >= loginSessionContext.ExpiresAt.AddMinutes(-_tokenExpiryBufferInMinute))
        {
            var newTokenQuery = await _osuAuthService.RefreshTokenAsync(loginSessionContext.RefreshToken);

            if (newTokenQuery == null)
            {
                await _sessionService.DeleteSessionAsync(sessionId);
                Response.Cookies.Delete(AppDefaults.SessionIdCookieName);
                return AuthenticateResult.NoResult();
            }

            loginSessionContext = new LoginSessionContext
            {
                UserId = loginSessionContext.UserId,
                OsuId = loginSessionContext.OsuId,
                Role = (await _userRepository.GetUserRoleAsync(loginSessionContext.OsuId))?.Role ?? "User",
                AccessToken = newTokenQuery.AccessToken,
                RefreshToken = newTokenQuery.RefreshToken,
                ExpiresAt = newTokenQuery.ExpiresAt
            };

            await _sessionService.SaveSessionAsync(sessionId, loginSessionContext);

            Response.Cookies.Append(
                AppDefaults.SessionIdCookieName,
                sessionId,
                DateTimeOffset.UtcNow.AddDays(_cookieSessionExpiryInDay));
        }

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, loginSessionContext.UserId.ToString()),
            new Claim(ClaimTypes.Role, loginSessionContext.Role),
            new Claim(CustomClaimTypes.OsuId, loginSessionContext.OsuId.ToString()),
            new Claim(CustomClaimTypes.AccessToken, loginSessionContext.AccessToken)
        };

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        return AuthenticateResult.Success(new AuthenticationTicket(principal, Scheme.Name));
    }

    protected override Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        Response.StatusCode = StatusCodes.Status401Unauthorized;
        return Task.CompletedTask;
    }
}
