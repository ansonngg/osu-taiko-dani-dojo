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
    IMemoryCache memoryCache)
    : ControllerBase
{
    private static readonly TimeSpan ExamSessionExpiry = TimeSpan.FromMinutes(60);
    private static readonly TimeSpan ExamSessionStatusCheckInterval = TimeSpan.FromSeconds(1);
    private readonly IOsuMultiplayerRoomService _osuMultiplayerRoomService = osuMultiplayerRoomService;
    private readonly ISessionService _sessionService = sessionService;
    private readonly IExamRepository _examRepository = examRepository;
    private readonly IExamSessionRepository _examSessionRepository = examSessionRepository;
    private readonly IMemoryCache _memoryCache = memoryCache;

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
        var multiplayerRoomQuery = await _osuMultiplayerRoomService.GetMostRecentActiveRoomAsync();

        if (!multiplayerRoomQuery.IsActive)
        {
            return BadRequest(new ExamSessionResponse { IsRoomActive = false });
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

        var examSessionQuery = await _examSessionRepository.CreateAsync(sessionContext.UserId, grade);

        var examSessionContext = new ExamSessionContext
        {
            ExamSessionId = examSessionQuery.ExamSessionId,
            RoomId = multiplayerRoomQuery.RoomId,
            StartedAt = examSessionQuery.StartedAt,
            ExamTracker = new ExamTracker(examQuery, roomPlaylistQuery.PlaylistIds, roomPlaylistQuery.TotalLengths)
        };

        _memoryCache.SetTyped(examSessionQuery.ExamSessionId, examSessionContext, ExamSessionExpiry);

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
                Id = examSessionQuery.ExamSessionId,
                IsRoomActive = true,
                IsPlaylistCorrect = isPlaylistCorrect
            });
    }

    [HttpGet("{examSessionId:int}")]
    public async Task<IActionResult> GetExamSessionEvent(int examSessionId)
    {
        if (!_memoryCache.TryGetTyped<ExamSessionContext>(examSessionId, out var examSessionContext))
        {
            return NotFound();
        }

        if (examSessionContext == null)
        {
            throw new NullReferenceException("Exam session context is null.");
        }

        Response.Headers.Append("Content-Type", "text/event-stream");
        var status = examSessionContext.Status;
        await _SendExamSessionStreamAsync(examSessionId, examSessionContext.ExamTracker.CurrentStage, status);
        var timer = new PeriodicTimer(ExamSessionStatusCheckInterval);

        while (await timer.WaitForNextTickAsync()
               && examSessionContext.Status is ExamSessionStatus.Waiting or ExamSessionStatus.Playing)
        {
            if (examSessionContext.Status == status)
            {
                continue;
            }

            status = examSessionContext.Status;
            await _SendExamSessionStreamAsync(examSessionId, examSessionContext.ExamTracker.CurrentStage, status);
        }

        await _SendExamSessionStreamAsync(examSessionId, examSessionContext.ExamTracker.CurrentStage, status);
        return Ok();
    }

    private async Task _SendExamSessionStreamAsync(int examSessionId, int stage, ExamSessionStatus status)
    {
        var maxWaitingTime = status == ExamSessionStatus.Waiting ? ClientConst.OsuPollingDuration.Seconds + "s" : "N/A";

        await Response.WriteAsync(
            $"""
             id: {examSessionId}
             stage: {stage}
             status: {status.ToString()}
             max_waiting_time: {maxWaitingTime}

             """);

        await Response.Body.FlushAsync();
    }
}
