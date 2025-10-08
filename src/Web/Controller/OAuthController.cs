using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using OsuTaikoDaniDojo.Application.Interface;
using OsuTaikoDaniDojo.Web.Context;
using OsuTaikoDaniDojo.Web.Request;
using OsuTaikoDaniDojo.Web.Response;
using OsuTaikoDaniDojo.Web.Utility;
using SessionOptions = OsuTaikoDaniDojo.Application.Options.SessionOptions;

namespace OsuTaikoDaniDojo.Web.Controller;

[ApiController]
[Route("api/[controller]")]
public class OAuthController(
    IOsuAuthService osuAuthService,
    ISessionService sessionService,
    IUserRepository userRepository,
    IOptions<SessionOptions> options,
    ILogger<OAuthController> logger)
    : ControllerBase
{
    private readonly IOsuAuthService _osuAuthService = osuAuthService;
    private readonly ISessionService _sessionService = sessionService;
    private readonly IUserRepository _userRepository = userRepository;
    private readonly int _cookieSessionExpiryInDay = options.Value.CookieExpiryInDay;
    private readonly ILogger<OAuthController> _logger = logger;

    [HttpGet("url")]
    public IActionResult GetAuthorizeUrl()
    {
        var url = _osuAuthService.GetAuthorizeUrl();
        return Ok(new AuthorizeUrlResponse { Url = url });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var tokenQuery = await _osuAuthService.ExchangeTokenAsync(request.Code);
        var osuId = await _osuAuthService.GetUserIdAsync(tokenQuery.AccessToken);
        var userRoleQuery = await _userRepository.GetUserRoleAsync(osuId) ?? await _userRepository.CreateAsync(osuId);

        var sessionData = new SessionContext
        {
            UserId = userRoleQuery.UserId,
            OsuId = osuId,
            Role = userRoleQuery.Role,
            AccessToken = tokenQuery.AccessToken,
            RefreshToken = tokenQuery.RefreshToken,
            ExpiresAt = tokenQuery.ExpiresAt
        };

        var sessionId = await _GenerateUniqueSessionIdAsync();
        await _sessionService.SaveSessionAsync(sessionId, sessionData);

        Response.Cookies.Append(
            ClientConst.SessionIdCookieName,
            sessionId,
            DateTimeOffset.UtcNow.AddDays(_cookieSessionExpiryInDay));

        _logger.LogInformation("User with Id {OsuId} logged in.", osuId);
        return Ok();
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var sessionId = Request.Cookies[ClientConst.SessionIdCookieName];

        if (string.IsNullOrEmpty(sessionId))
        {
            return BadRequest();
        }

        await _sessionService.DeleteSessionAsync(sessionId);
        Response.Cookies.Delete(ClientConst.SessionIdCookieName);
        return Ok();
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
