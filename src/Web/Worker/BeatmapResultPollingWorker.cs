using OsuTaikoDaniDojo.Application.Interface;
using OsuTaikoDaniDojo.Web.Context;
using OsuTaikoDaniDojo.Web.Utility;

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

    protected override async Task Execute()
    {
        var accessToken = (await _sessionService.GetSessionAsync<SessionContext>(_sessionId))?.AccessToken;

        if (string.IsNullOrEmpty(accessToken))
        {
            await _examSessionRepository.TerminateAsync(_examSessionContext.ExamSessionId);
            IsCanceling = true;
            return;
        }

        _osuMultiplayerRoomService.SetAuthenticationHeader(accessToken);

        var beatmapResultQuery = await _osuMultiplayerRoomService.GetBeatmapResultAsync(
            _examSessionContext.RoomId,
            _examSessionContext.ExamTracker.CurrentPlaylistId);

        if (beatmapResultQuery == null)
        {
            return;
        }

        if (!_examSessionContext.ExamTracker.Judge(beatmapResultQuery))
        {
            await _examSessionRepository.SetCompletedAsync(_examSessionContext.ExamSessionId);
        }
        else if (_examSessionContext.ExamTracker.IsEnded)
        {
            await _examSessionRepository.SetCompletedAsync(_examSessionContext.ExamSessionId);
        }
        else
        {
            await _examSessionRepository.ProceedToNextStageAsync(_examSessionContext.ExamSessionId);

            new PlaylistStatusPollingWorker(
                    _osuMultiplayerRoomService,
                    _sessionService,
                    _examSessionRepository,
                    _sessionId,
                    _examSessionContext)
                .Run(ClientConst.OsuPollingInterval, ClientConst.OsuPollingDuration);
        }

        IsCanceling = true;
    }

    protected override async Task OnCompleted()
    {
        if (!IsCanceling)
        {
            await _examSessionRepository.SetNoResponseAsync(_examSessionContext.ExamSessionId);
        }
    }
}
