using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using OsuTaikoDaniDojo.Application.Interface;
using OsuTaikoDaniDojo.Application.Model;
using OsuTaikoDaniDojo.Application.Utility;
using OsuTaikoDaniDojo.Presentation.Request;
using OsuTaikoDaniDojo.Presentation.Response;
using OsuTaikoDaniDojo.Presentation.Utility;
using SessionOptions = OsuTaikoDaniDojo.Application.Options.SessionOptions;

namespace OsuTaikoDaniDojo.Presentation.Controller;

[ApiController]
[Route("api/[controller]")]
public class OAuthController(
    IOsuAuthService osuAuthService,
    ISessionService sessionService,
    IOptions<SessionOptions> options)
    : ControllerBase
{
    private readonly IOsuAuthService _osuAuthService = osuAuthService;
    private readonly ISessionService _sessionService = sessionService;
    private readonly int _cookieSessionExpiryInDay = options.Value.CookieExpiryInDay;

    [HttpGet("authorize-url")]
    public IActionResult GetAuthorizeUrl()
    {
        var url = _osuAuthService.GetAuthorizeUrl();
        return Ok(new AuthorizeUrlResponse { Url = url });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var userToken = await _osuAuthService.ExchangeTokenAsync(request.Code);
        var userId = await _osuAuthService.GetUserIdAsync(userToken.AccessToken);
        var sessionData = new UserSession { UserId = userId, UserToken = userToken };
        var sessionId = await _GenerateUniqueSessionIdAsync();
        await _sessionService.SaveSessionAsync(sessionId, sessionData);

        Response.Cookies.Append(
            ClientConst.SessionIdCookieName,
            sessionId,
            DateTimeOffset.UtcNow.AddDays(_cookieSessionExpiryInDay));

        this.Log($"User with Id {userId} logged in.");
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
