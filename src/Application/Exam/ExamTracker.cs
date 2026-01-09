using OsuTaikoDaniDojo.Application.Query;
using OsuTaikoDaniDojo.Domain.Entity;
using OsuTaikoDaniDojo.Domain.Exam;
using OsuTaikoDaniDojo.Domain.Utility;

namespace OsuTaikoDaniDojo.Application.Exam;

public class ExamTracker
{
    private readonly ExamBeatmap[] _examBeatmaps;
    private readonly PassEvaluator _passEvaluator = new();
    private int _currentStage;

    public ExamTracker(ExamQuery examQuery, int[] playlistIds, int[] totalLengths)
    {
        var beatmapCount = examQuery.BeatmapIds.Length;
        _examBeatmaps = new ExamBeatmap[beatmapCount];

        for (var i = 0; i < beatmapCount; i++)
        {
            _examBeatmaps[i] = new ExamBeatmap(examQuery.BeatmapIds[i], playlistIds[i], totalLengths[i]);

            _examBeatmaps[i].SetUpCriteria(
                examQuery.SpecificGreatCounts?.Split(beatmapCount, i),
                examQuery.SpecificOkCounts?.Split(beatmapCount, i),
                examQuery.SpecificMissCounts?.Split(beatmapCount, i),
                examQuery.SpecificLargeBonusCounts?.Split(beatmapCount, i),
                examQuery.SpecificMaxCombos?.Split(beatmapCount, i),
                examQuery.SpecificHitCounts?.Split(beatmapCount, i));
        }

        _passEvaluator.SetUpCriteria(
            examQuery.GeneralGreatCounts,
            examQuery.GeneralOkCounts,
            examQuery.GeneralMissCounts,
            examQuery.GeneralLargeBonusCounts,
            examQuery.GeneralMaxCombos,
            examQuery.GeneralHitCounts);
    }

    public int CurrentStage => Math.Min(_currentStage + 1, _examBeatmaps.Length);
    public int CurrentPlaylistId => !IsEnded ? _examBeatmaps[_currentStage].PlaylistId : 0;
    public int CurrentBeatmapId => !IsEnded ? _examBeatmaps[_currentStage].Id : 0;
    public int CurrentBeatmapLength => !IsEnded ? _examBeatmaps[_currentStage].Length : 0;
    public bool IsEnded => _currentStage >= _examBeatmaps.Length;
    public int[] GreatCounts => _examBeatmaps.Select(x => x.Result.GreatCount).ToArray();
    public int[] OkCounts => _examBeatmaps.Select(x => x.Result.OkCount).ToArray();
    public int[] MissCounts => _examBeatmaps.Select(x => x.Result.MissCount).ToArray();
    public int[] LargeBonusCounts => _examBeatmaps.Select(x => x.Result.LargeBonusCount).ToArray();
    public int[] MaxCombos => _examBeatmaps.Select(x => x.Result.MaxCombo).ToArray();
    public int[] HitCounts => _examBeatmaps.Select(x => x.Result.HitCount).ToArray();
    public int PassLevel { get; private set; }

    public bool IsModListValid(string[] mods)
    {
        // TODO: Might implement real mod list checking later
        return mods.Length == 0;
    }

    public bool Judge(BeatmapResultQuery beatmapResultQuery)
    {
        if (IsEnded)
        {
            throw new InvalidOperationException("Exam has already ended.");
        }

        var stageResult = new StageResult
        {
            GreatCount = beatmapResultQuery.GreatCount,
            OkCount = beatmapResultQuery.OkCount,
            MissCount = beatmapResultQuery.MissCount,
            LargeBonusCount = beatmapResultQuery.LargeBonusCount,
            MaxCombo = beatmapResultQuery.MaxCombo,
            HitCount = beatmapResultQuery.HitCount
        };

        var beatmapPassLevel = _examBeatmaps[_currentStage].Evaluate(stageResult);
        PassLevel = _currentStage == 0 ? beatmapPassLevel : Math.Min(PassLevel, beatmapPassLevel);

        if (PassLevel == 0)
        {
            return false;
        }

        _currentStage++;

        if (!IsEnded)
        {
            return true;
        }

        stageResult = new StageResult
        {
            GreatCount = _examBeatmaps.Select(x => x.Result.GreatCount).Sum(),
            OkCount = _examBeatmaps.Select(x => x.Result.OkCount).Sum(),
            MissCount = _examBeatmaps.Select(x => x.Result.MissCount).Sum(),
            LargeBonusCount = _examBeatmaps.Select(x => x.Result.LargeBonusCount).Sum(),
            MaxCombo = _examBeatmaps.Select(x => x.Result.MaxCombo).Sum(),
            HitCount = _examBeatmaps.Select(x => x.Result.HitCount).Sum()
        };

        PassLevel = Math.Min(PassLevel, _passEvaluator.Evaluate(stageResult));
        return PassLevel > 0;
    }
}
