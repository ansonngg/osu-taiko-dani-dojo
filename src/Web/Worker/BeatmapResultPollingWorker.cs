using OsuTaikoDaniDojo.Application.Interface;
using OsuTaikoDaniDojo.Web.Context;
using OsuTaikoDaniDojo.Web.Utility;

namespace OsuTaikoDaniDojo.Web.Worker;

public class BeatmapResultPollingWorker(
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
        var beatmapResultQuery = await _osuMultiplayerRoomService.GetBeatmapResultAsync(
            _examSessionContext.RoomId,
            _examSessionContext.ExamTracker.CurrentPlaylistId,
            _accessToken);

        if (beatmapResultQuery == null)
        {
            return;
        }

        if (beatmapResultQuery.HasMods)
        {
            await _examSessionRepository.DisqualifyAsync(_examSessionContext.ExamSessionId);
            _examSessionContext.Status = ExamSessionStatus.Disqualified;
        }
        else if (!_examSessionContext.ExamTracker.Judge(beatmapResultQuery))
        {
            await _examSessionRepository.SetCompletedAsync(_examSessionContext.ExamSessionId);
            _examSessionContext.Status = ExamSessionStatus.Failed;
        }
        else if (_examSessionContext.ExamTracker.IsEnded)
        {
            await _examSessionRepository.SetCompletedAsync(_examSessionContext.ExamSessionId);
            _examSessionContext.Status = ExamSessionStatus.Passed;

            var gradeCertificateId = await _gradeCertificateRepository.CreateAsync(
                _examSessionContext.UserId,
                _examSessionContext.Grade,
                _examSessionContext.ExamTracker.PassLevel,
                _examSessionContext.ExamTracker.GreatCounts,
                _examSessionContext.ExamTracker.OkCounts,
                _examSessionContext.ExamTracker.MissCounts,
                _examSessionContext.ExamTracker.LargeBonusCounts,
                _examSessionContext.ExamTracker.MaxCombos,
                _examSessionContext.ExamTracker.HitCounts,
                _examSessionContext.ExamSessionId);

            await _userRepository.UpdateHighestGradeCertificateAsync(_examSessionContext.UserId, gradeCertificateId);
        }
        else
        {
            await _examSessionRepository.ProceedToNextStageAsync(_examSessionContext.ExamSessionId);
            _examSessionContext.Status = ExamSessionStatus.Waiting;

            new PlaylistStatusPollingWorker(
                    _osuMultiplayerRoomService,
                    _examSessionRepository,
                    _gradeCertificateRepository,
                    _userRepository,
                    _accessToken,
                    _examSessionContext)
                .Run(ClientConst.OsuPollingInterval, ClientConst.OsuPollingDuration);
        }

        Cancel();
    }

    protected override async Task OnCompleted()
    {
        if (_examSessionContext.Status != ExamSessionStatus.Playing)
        {
            return;
        }

        await _examSessionRepository.SetNoResponseAsync(_examSessionContext.ExamSessionId);
        _examSessionContext.Status = ExamSessionStatus.NoResponse;
    }
}
