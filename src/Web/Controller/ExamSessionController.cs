using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using OsuTaikoDaniDojo.Application.Interface;
using OsuTaikoDaniDojo.Application.System;
using OsuTaikoDaniDojo.Application.Utility;
using OsuTaikoDaniDojo.Web.Context;
using OsuTaikoDaniDojo.Web.Response;
using OsuTaikoDaniDojo.Web.Utility;
using OsuTaikoDaniDojo.Web.Worker;

namespace OsuTaikoDaniDojo.Web.Controller;

[ApiController]
[Route("api/[controller]")]
public class ExamSessionController(
    IOsuMultiplayerRoomService osuMultiplayerRoomService,
    ISessionService sessionService,
    IExamRepository examRepository,
    IExamSessionRepository examSessionRepository,
    IMemoryCache memoryCache,
    ILogger<ExamSessionController> logger)
    : ControllerBase
{
    private static readonly TimeSpan ExamSessionExpiry = TimeSpan.FromMinutes(60);
    private static readonly TimeSpan ExamSessionStatusCheckInterval = TimeSpan.FromSeconds(1);
    private readonly IOsuMultiplayerRoomService _osuMultiplayerRoomService = osuMultiplayerRoomService;
    private readonly ISessionService _sessionService = sessionService;
    private readonly IExamRepository _examRepository = examRepository;
    private readonly IExamSessionRepository _examSessionRepository = examSessionRepository;
    private readonly IMemoryCache _memoryCache = memoryCache;
    private readonly ILogger<ExamSessionController> _logger = logger;

    [HttpPost("grade/{grade:int}")]
    public async Task<IActionResult> StartExamSession(int grade)
    {
        var examQuery = await _examRepository.GetExamByGradeAsync(grade);

        if (examQuery == null)
        {
            return NotFound();
        }

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

        _osuMultiplayerRoomService.SetAuthenticationHeader(sessionContext.AccessToken);
        var multiplayerRoomQuery = await _osuMultiplayerRoomService.GetMostRecentActiveRoomAsync(sessionContext.UserId);

        if (multiplayerRoomQuery is not { IsActive: true } || multiplayerRoomQuery.Status != "idle")
        {
            return BadRequest(new ExamSessionResponse { IsRoomActive = false });
        }

        if (multiplayerRoomQuery.ActivePlaylistCount != examQuery.BeatmapIds.Length)
        {
            return BadRequest(new ExamSessionResponse { IsRoomActive = true });
        }

        var roomPlaylistQuery = await _osuMultiplayerRoomService.GetRoomPlaylistAsync(multiplayerRoomQuery.RoomId);
        var isPlaylistCorrect = new bool[examQuery.BeatmapIds.Length];
        var isExamValid = true;

        for (var i = 0; i < examQuery.BeatmapIds.Length; i++)
        {
            if (roomPlaylistQuery.BeatmapIds[^(examQuery.BeatmapIds.Length - i)] == examQuery.BeatmapIds[i])
            {
                isPlaylistCorrect[i] = true;
            }
            else
            {
                isPlaylistCorrect[i] = false;
                isExamValid = false;
            }
        }

        if (!isExamValid)
        {
            return BadRequest(new ExamSessionResponse { IsRoomActive = true, IsPlaylistCorrect = isPlaylistCorrect });
        }

        var examSessionId = await _examSessionRepository.CreateAsync(sessionContext.UserId, grade);

        var examSessionContext = new ExamSessionContext
        {
            ExamSessionId = examSessionId,
            UserId = sessionContext.UserId,
            RoomId = multiplayerRoomQuery.RoomId,
            ExamTracker = new ExamTracker(
                examQuery,
                roomPlaylistQuery.PlaylistIds[^examQuery.BeatmapIds.Length..],
                roomPlaylistQuery.TotalLengths[^examQuery.BeatmapIds.Length..])
        };

        _memoryCache.SetTyped(examSessionId, examSessionContext, ExamSessionExpiry);

        new PlaylistStatusPollingWorker(
                _osuMultiplayerRoomService,
                _sessionService,
                _examSessionRepository,
                sessionId,
                examSessionContext)
            .Run(ClientConst.OsuPollingInterval, ClientConst.OsuPollingDuration);

        return Ok(
            new ExamSessionResponse
            {
                Id = examSessionId,
                IsRoomActive = true,
                IsPlaylistCorrect = isPlaylistCorrect
            });
    }

    [HttpGet("{examSessionId:int}")]
    public async Task GetExamSessionEvent(int examSessionId)
    {
        if (!_memoryCache.TryGetTyped<ExamSessionContext>(examSessionId, out var examSessionContext))
        {
            Response.StatusCode = 404;
            return;
        }

        if (examSessionContext == null)
        {
            throw new NullReferenceException("Exam session context is null.");
        }

        var sessionId = Request.Cookies[ClientConst.SessionIdCookieName];

        if (string.IsNullOrEmpty(sessionId))
        {
            Response.StatusCode = 401;
            return;
        }

        var sessionContext = await _sessionService.GetSessionAsync<SessionContext>(sessionId);

        if (sessionContext == null || sessionContext.UserId != examSessionContext.UserId)
        {
            Response.StatusCode = 401;
            return;
        }

        Response.ContentType = "text/event-stream";
        Response.Headers.CacheControl = "no-cache";
        Response.Headers.Connection = "keep-alive";
        await _SendExamSessionStreamAsync(examSessionId, examSessionContext);

        var status = examSessionContext.Status;
        var timer = new PeriodicTimer(ExamSessionStatusCheckInterval);

        try
        {
            while (await timer.WaitForNextTickAsync()
                   && examSessionContext.Status is ExamSessionStatus.Waiting or ExamSessionStatus.Playing)
            {
                HttpContext.RequestAborted.ThrowIfCancellationRequested();

                if (examSessionContext.Status == status)
                {
                    continue;
                }

                await _SendExamSessionStreamAsync(examSessionId, examSessionContext);
                status = examSessionContext.Status;
            }

            await _SendExamSessionStreamAsync(examSessionId, examSessionContext);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Client disconnected from exam session {ExamSessionId}.", examSessionId);
        }
    }

    private async Task _SendExamSessionStreamAsync(int examSessionId, ExamSessionContext examSessionContext)
    {
        int? maxWaitingTime = examSessionContext.Status switch
        {
            ExamSessionStatus.Waiting => ClientConst.OsuPollingDuration.Seconds,
            ExamSessionStatus.Playing => examSessionContext.ExamTracker.CurrentBeatmapLength
                                         + ClientConst.OsuPollingDuration.Seconds,
            _ => null
        };

        var payload = JsonSerializer.Serialize(
            new
            {
                stage = examSessionContext.ExamTracker.CurrentStage,
                status = examSessionContext.Status.ToSnakeCase(),
                max_waiting_time = maxWaitingTime,
                pass_level = examSessionContext.Status == ExamSessionStatus.Passed
                    ? (int?)examSessionContext.ExamTracker.PassLevel
                    : null
            });

        await Response.WriteAsync($"id: {examSessionId}\n");
        await Response.WriteAsync($"data: {payload}\n\n");
        await Response.Body.FlushAsync();
    }
}
