using Microsoft.AspNetCore.Mvc;
using OsuTaikoDaniDojo.Application.Interface;
using OsuTaikoDaniDojo.Application.Model;
using OsuTaikoDaniDojo.Application.Utility;
using OsuTaikoDaniDojo.Presentation.Request;
using OsuTaikoDaniDojo.Presentation.Response;
using OsuTaikoDaniDojo.Presentation.Utility;

namespace OsuTaikoDaniDojo.Presentation.Controller;

[ApiController]
[Route("api/[controller]")]
public class OAuthController(IOsuAuthService osuAuthService, IRedisSessionService redisSessionService) : ControllerBase
{
    private readonly IOsuAuthService _osuAuthService = osuAuthService;
    private readonly IRedisSessionService _redisSessionService = redisSessionService;

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
        await _redisSessionService.SaveSessionAsync(sessionId, sessionData, ClientConst.SessionIdCookieExpiryInSecond);

        Response.Cookies.Append(
            ClientConst.SessionIdCookieName,
            sessionId,
            DateTimeOffset.UtcNow.AddSeconds(ClientConst.SessionIdCookieExpiryInSecond));

        this.Log($"User with Id {userId} logged in.");
        return Ok();
    }

    private async Task<string> _GenerateUniqueSessionIdAsync()
    {
        var sessionId = Guid.NewGuid().ToString();

        while (await _redisSessionService.ExistsSessionAsync(sessionId))
        {
            sessionId = Guid.NewGuid().ToString();
        }

        return sessionId;
    }
}
