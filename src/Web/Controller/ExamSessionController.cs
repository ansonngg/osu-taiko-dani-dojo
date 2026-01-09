using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using OsuTaikoDaniDojo.Application.Exam;
using OsuTaikoDaniDojo.Application.Interface;
using OsuTaikoDaniDojo.Application.Utility;
using OsuTaikoDaniDojo.Web.Const;
using OsuTaikoDaniDojo.Web.Context;
using OsuTaikoDaniDojo.Web.Response;
using OsuTaikoDaniDojo.Web.Utility;
using OsuTaikoDaniDojo.Web.Worker;

namespace OsuTaikoDaniDojo.Web.Controller;

[ApiController]
[Route("api/[controller]")]
public class ExamSessionController(
    IOsuMultiplayerRoomService osuMultiplayerRoomService,
    IExamRepository examRepository,
    IExamSessionRepository examSessionRepository,
    IGradeCertificateRepository gradeCertificateRepository,
    IUserRepository userRepository,
    IMemoryCache memoryCache,
    ILogger<ExamSessionController> logger)
    : ControllerBase
{
    private static readonly TimeSpan ExamSessionExpiry = TimeSpan.FromMinutes(60);
    private static readonly TimeSpan ExamSessionStatusCheckInterval = TimeSpan.FromSeconds(1);
    private readonly IOsuMultiplayerRoomService _osuMultiplayerRoomService = osuMultiplayerRoomService;
    private readonly IExamRepository _examRepository = examRepository;
    private readonly IExamSessionRepository _examSessionRepository = examSessionRepository;
    private readonly IGradeCertificateRepository _gradeCertificateRepository = gradeCertificateRepository;
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IMemoryCache _memoryCache = memoryCache;
    private readonly ILogger<ExamSessionController> _logger = logger;

    [HttpPost("grade/{grade:int}")]
    public async Task<IActionResult> StartExamSession(int grade)
    {
        var examQuery = await _examRepository.GetByGradeAsync(grade);

        if (examQuery == null)
        {
            return NotFound();
        }

        var accessToken = User.FindFirstValue(CustomClaimTypes.AccessToken);

        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId)
            || !int.TryParse(User.FindFirstValue(CustomClaimTypes.OsuId), out var osuId)
            || string.IsNullOrEmpty(accessToken))
        {
            return Unauthorized();
        }

        var inProgressExamSessionId = await _examSessionRepository.GetInProgressIdAsync(osuId);

        if (inProgressExamSessionId != null)
        {
            return Conflict(new ExamSessionResponse { Id = inProgressExamSessionId.Value });
        }

        var multiplayerRoomQuery = await _osuMultiplayerRoomService.GetMostRecentActiveRoomAsync(osuId);

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

        var examSessionId = await _examSessionRepository.CreateAsync(osuId, grade);

        var examSessionContext = new ExamSessionContext
        {
            ExamSessionId = examSessionId,
            UserId = userId,
            OsuId = osuId,
            Grade = grade,
            RoomId = multiplayerRoomQuery.RoomId,
            ExamTracker = new ExamTracker(
                examQuery,
                roomPlaylistQuery.PlaylistIds[^examQuery.BeatmapIds.Length..],
                roomPlaylistQuery.TotalLengths[^examQuery.BeatmapIds.Length..])
        };

        _memoryCache.SetTyped(examSessionId, examSessionContext, ExamSessionExpiry);

        new PlaylistStatusPollingWorker(
                _osuMultiplayerRoomService,
                _examSessionRepository,
                _gradeCertificateRepository,
                _userRepository,
                accessToken,
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

        var accessToken = User.FindFirstValue(CustomClaimTypes.AccessToken);

        if (string.IsNullOrEmpty(accessToken))
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
                osu_id = examSessionContext.OsuId,
                grade = examSessionContext.Grade,
                stage = examSessionContext.ExamTracker.CurrentStage,
                great_counts = examSessionContext.ExamTracker.GreatCounts,
                ok_counts = examSessionContext.ExamTracker.OkCounts,
                miss_counts = examSessionContext.ExamTracker.MissCounts,
                large_bonus_counts = examSessionContext.ExamTracker.LargeBonusCounts,
                max_combos = examSessionContext.ExamTracker.MaxCombos,
                hit_counts = examSessionContext.ExamTracker.HitCounts,
                status = examSessionContext.Status.ToSnakeCase(),
                timeout_at = maxWaitingTime.HasValue
                    ? (DateTime?)examSessionContext.LastUpdatedAt + TimeSpan.FromSeconds(maxWaitingTime.Value)
                    : null,
                pass_level = examSessionContext.Status == ExamSessionStatus.Passed
                    ? (int?)examSessionContext.ExamTracker.PassLevel
                    : null
            });

        await Response.WriteAsync($"id: {examSessionId}\n");
        await Response.WriteAsync($"data: {payload}\n\n");
        await Response.Body.FlushAsync();
    }
}
