using OsuTaikoDaniDojo.Application.Interface;
using OsuTaikoDaniDojo.Web.Context;
using OsuTaikoDaniDojo.Web.Utility;

namespace OsuTaikoDaniDojo.Web.Worker;

public class PlaylistStatusPollingWorker(
    IOsuMultiplayerRoomService osuMultiplayerRoomService,
    ISessionService sessionService,
    IExamSessionRepository examSessionRepository,
    string sessionId,
    ExamSessionContext examSessionContext)
    : WorkerBase
{
    private readonly IOsuMultiplayerRoomService _osuMultiplayerRoomService = osuMultiplayerRoomService;
    private readonly ISessionService _sessionService = sessionService;
    private readonly IExamSessionRepository _examSessionRepository = examSessionRepository;
    private readonly string _sessionId = sessionId;
    private readonly ExamSessionContext _examSessionContext = examSessionContext;

    protected override async Task Execute()
    {
        var sessionContext = await _sessionService.GetSessionAsync<SessionContext>(_sessionId);

        if (sessionContext == null)
        {
            await _examSessionRepository.TerminateAsync(_examSessionContext.ExamSessionId);
            _examSessionContext.Status = ExamSessionStatus.Terminated;
            Cancel();
            return;
        }

        _osuMultiplayerRoomService.SetAuthenticationHeader(sessionContext.AccessToken);
        var multiplayerRoomQuery = await _osuMultiplayerRoomService.GetMostRecentActiveRoomAsync(sessionContext.UserId);

        if (multiplayerRoomQuery == null
            || multiplayerRoomQuery.RoomId != _examSessionContext.RoomId
            || !multiplayerRoomQuery.IsActive
            || multiplayerRoomQuery.CurrentPlaylistId != _examSessionContext.ExamTracker.CurrentPlaylistId
            || multiplayerRoomQuery.CurrentBeatmapId != _examSessionContext.ExamTracker.CurrentBeatmapId)
        {
            await _examSessionRepository.DisqualifyAsync(_examSessionContext.ExamSessionId);
            _examSessionContext.Status = ExamSessionStatus.Disqualified;
            Cancel();
            return;
        }

        if (multiplayerRoomQuery.Status == "idle")
        {
            return;
        }

        _examSessionContext.Status = ExamSessionStatus.Playing;

        new BeatmapResultPollingWorker(
                _osuMultiplayerRoomService,
                _sessionService,
                _examSessionRepository,
                _sessionId,
                _examSessionContext)
            .Run(
                ClientConst.OsuPollingInterval,
                ClientConst.OsuPollingDuration,
                TimeSpan.FromSeconds(_examSessionContext.ExamTracker.CurrentBeatmapLength));

        Cancel();
    }

    protected override async Task OnCompleted()
    {
        if (_examSessionContext.Status != ExamSessionStatus.Waiting)
        {
            return;
        }

        await _examSessionRepository.SetTimeOutAsync(_examSessionContext.ExamSessionId);
        _examSessionContext.Status = ExamSessionStatus.TimeOut;
    }
}
