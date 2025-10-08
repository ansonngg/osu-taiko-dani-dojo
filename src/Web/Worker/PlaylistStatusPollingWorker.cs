using OsuTaikoDaniDojo.Application.Interface;
using OsuTaikoDaniDojo.Web.Context;
using OsuTaikoDaniDojo.Web.Utility;

namespace OsuTaikoDaniDojo.Web.Worker;

public class PlaylistStatusPollingWorker(
    IOsuMultiplayerRoomService osuMultiplayerRoomService,
    IExamSessionRepository examSessionRepository,
    int userId,
    string accessToken,
    ExamSessionContext examSessionContext)
    : WorkerBase
{
    private readonly IOsuMultiplayerRoomService _osuMultiplayerRoomService = osuMultiplayerRoomService;
    private readonly IExamSessionRepository _examSessionRepository = examSessionRepository;
    private readonly int _userId = userId;
    private readonly string _accessToken = accessToken;
    private readonly ExamSessionContext _examSessionContext = examSessionContext;

    protected override async Task Execute()
    {
        var multiplayerRoomQuery = await _osuMultiplayerRoomService.GetMostRecentActiveRoomAsync(_userId, _accessToken);

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
                _examSessionRepository,
                _userId,
                _accessToken,
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
