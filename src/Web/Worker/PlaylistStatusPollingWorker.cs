using OsuTaikoDaniDojo.Application.Interface;
using OsuTaikoDaniDojo.Web.Const;
using OsuTaikoDaniDojo.Web.Context;

namespace OsuTaikoDaniDojo.Web.Worker;

public class PlaylistStatusPollingWorker(
    IOsuMultiplayerRoomService osuMultiplayerRoomService,
    IExamSessionRepository examSessionRepository,
    IGradeCertificateRepository gradeCertificateRepository,
    IUserRepository userRepository,
    string accessToken,
    ExamSessionContext examSessionContext)
    : WorkerBase
{
    private readonly IOsuMultiplayerRoomService _osuMultiplayerRoomService = osuMultiplayerRoomService;
    private readonly IExamSessionRepository _examSessionRepository = examSessionRepository;
    private readonly IGradeCertificateRepository _gradeCertificateRepository = gradeCertificateRepository;
    private readonly IUserRepository _userRepository = userRepository;
    private readonly string _accessToken = accessToken;
    private readonly ExamSessionContext _examSessionContext = examSessionContext;

    protected override async Task Execute()
    {
        var multiplayerRoomQuery = await _osuMultiplayerRoomService.GetMostRecentActiveRoomAsync(
            _examSessionContext.OsuId,
            _accessToken);

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
                _gradeCertificateRepository,
                _userRepository,
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
