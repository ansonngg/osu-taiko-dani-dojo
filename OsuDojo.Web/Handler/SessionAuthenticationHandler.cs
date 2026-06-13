using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using OsuDojo.Application.Context;
using OsuDojo.Application.Interface;
using OsuDojo.Application.Options;
using OsuDojo.Web.Const;
using OsuDojo.Web.Utility;

namespace OsuDojo.Web.Handler;

public class SessionAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    IOsuAuthService osuAuthService,
    ILoginService loginService,
    IUserRepository userRepository,
    IOptions<LoginSessionOptions> loginSessionOptions,
    IOptions<OsuOptions> osuOptions)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    private readonly IOsuAuthService _osuAuthService = osuAuthService;
    private readonly ILoginService _loginService = loginService;
    private readonly IUserRepository _userRepository = userRepository;
    private readonly int _cookieSessionExpiryInDay = loginSessionOptions.Value.CookieExpiryInDay;
    private readonly int _tokenExpiryBufferInMinute = osuOptions.Value.TokenExpiryBufferInMinute;

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var sessionId = Request.Cookies[AppDefaults.SessionIdCookieName];

        if (string.IsNullOrEmpty(sessionId))
        {
            return AuthenticateResult.NoResult();
        }

        var loginSessionContext = await _loginService.GetLoginSessionAsync(sessionId);

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
                await _loginService.DeleteLoginSessionAsync(sessionId);
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

            await _loginService.UpdateLoginSessionAsync(sessionId, loginSessionContext);

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
