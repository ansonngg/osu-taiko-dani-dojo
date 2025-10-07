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
    private bool _isStatusValid;

    protected override async Task Execute()
    {
        var accessToken = (await _sessionService.GetSessionAsync<SessionContext>(_sessionId))?.AccessToken;

        if (string.IsNullOrEmpty(accessToken))
        {
            Cancel();
            return;
        }

        _osuMultiplayerRoomService.SetAuthenticationHeader(accessToken);
        var multiplayerRoomQuery = await _osuMultiplayerRoomService.GetMostRecentActiveRoomAsync();

        if (multiplayerRoomQuery.RoomId != _examSessionContext.RoomId
            || !multiplayerRoomQuery.IsActive
            || multiplayerRoomQuery.CurrentPlaylistId
            != _examSessionContext.PlaylistIds[_examSessionContext.LastReachedStage - 1]
            || multiplayerRoomQuery.CurrentBeatmapId
            != _examSessionContext.ExamQuery.BeatmapIds[_examSessionContext.LastReachedStage - 1])
        {
            Cancel();
            return;
        }

        if (multiplayerRoomQuery.LastPlayedAt == null)
        {
            return;
        }

        if (multiplayerRoomQuery.LastPlayedAt >= _examSessionContext.StartedAt)
        {
            _isStatusValid = true;
        }

        Cancel();
    }

    protected override void OnCompleted()
    {
        if (!_isStatusValid)
        {
            _examSessionRepository.SetTimeOutAsync(_examSessionContext.ExamSessionId);
            return;
        }

        new BeatmapResultPollingWorker(
                _osuMultiplayerRoomService,
                _sessionService,
                _examSessionRepository,
                _sessionId,
                _examSessionContext)
            .Run(
                ClientConst.OsuPollingInterval,
                ClientConst.OsuPollingDuration,
                TimeSpan.FromSeconds(_examSessionContext.TotalLengths[_examSessionContext.LastReachedStage - 1]));
    }
}
