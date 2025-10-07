using Microsoft.AspNetCore.Mvc;
using OsuTaikoDaniDojo.Application.Interface;
using OsuTaikoDaniDojo.Web.Context;
using OsuTaikoDaniDojo.Web.Response;
using OsuTaikoDaniDojo.Web.Utility;

namespace OsuTaikoDaniDojo.Web.Controller;

[ApiController]
[Route("api/[controller]")]
public class ExamSessionController(ISessionService sessionService, IExamSessionRepository examSessionRepository)
    : ControllerBase
{
    private readonly ISessionService _sessionService = sessionService;
    private readonly IExamSessionRepository _examSessionRepository = examSessionRepository;

    [HttpPost("grade/{grade:int}")]
    public async Task<IActionResult> StartExamSession(int grade)
    {
        var sessionId = Request.Cookies[ClientConst.SessionIdCookieName];

        if (string.IsNullOrEmpty(sessionId))
        {
            return Unauthorized();
        }

        var sessionContext = await _sessionService.GetSessionAsync<SessionContext>(sessionId);

        if (sessionContext == null)
        {
            return Unauthorized();
        }

        var examSessionId = await _examSessionRepository.CreateAsync(sessionContext.UserId, grade);
        return Ok(new ExamSessionResponse { ExamSessionId = examSessionId });
    }
}
