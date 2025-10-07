using OsuTaikoDaniDojo.Application.Interface;
using OsuTaikoDaniDojo.Web.Context;

namespace OsuTaikoDaniDojo.Web.Worker;

public class BeatmapResultPollingWorker(
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

        var beatmapResultQuery = await _osuMultiplayerRoomService.GetBeatmapResultAsync(
            _examSessionContext.RoomId,
            _examSessionContext.PlaylistIds[_examSessionContext.LastReachedStage - 1]);

        if (beatmapResultQuery == null)
        {
            return;
        }
    }

    protected override void OnCompleted()
    {
        if (!_isStatusValid)
        {
            _examSessionRepository.SetNoResponseAsync(_examSessionContext.ExamSessionId);
            return;
        }
    }
}
