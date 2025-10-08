using OsuTaikoDaniDojo.Application.Query;
using OsuTaikoDaniDojo.Domain.Entity;

namespace OsuTaikoDaniDojo.Application.System;

public class ExamTracker
{
    private readonly ExamCriteria[] _specificCriteriaForBeatmap;
    private readonly ExamCriteria _generalCriteria = new();
    private readonly int[] _playlistIds;
    private readonly int[] _beatmapIds;
    private readonly int[] _totalLengths;
    private int _currentStage;
    private int _totalGreatCount;
    private int _totalOkCount;
    private int _totalMissCount;
    private int _totalLargeBonusCount;
    private int _totalMaxCombo;
    private int _totalHitCount;

    public ExamTracker(ExamQuery examQuery, int[] playlistIds, int[] totalLengths)
    {
        _specificCriteriaForBeatmap = new ExamCriteria[examQuery.BeatmapIds.Length];

        for (var i = 0; i < _specificCriteriaForBeatmap.Length; i++)
        {
            _specificCriteriaForBeatmap[i] = new ExamCriteria();
        }

        _playlistIds = playlistIds;
        _beatmapIds = examQuery.BeatmapIds;
        _totalLengths = totalLengths;

        var pendingSpecificCriteria = new[]
        {
            examQuery.SpecificGreatCounts,
            examQuery.SpecificOkCounts,
            examQuery.SpecificMissCounts,
            examQuery.SpecificLargeBonusCounts,
            examQuery.SpecificMaxCombos,
            examQuery.SpecificHitCounts
        };

        var pendingGeneralCriteria = new[]
        {
            examQuery.GeneralGreatCounts,
            examQuery.GeneralOkCounts,
            examQuery.GeneralMissCounts,
            examQuery.GeneralLargeBonusCounts,
            examQuery.GeneralMaxCombos,
            examQuery.GeneralHitCounts
        };

        const int passTypeCount = (int)ExamResult.Count - 1;

        for (var i = 0; i < pendingSpecificCriteria.Length; i++)
        {
            if (pendingSpecificCriteria[i] == null
                || pendingSpecificCriteria[i]!.Length != passTypeCount * _beatmapIds.Length)
            {
                if (pendingGeneralCriteria[i] is { Length: passTypeCount })
                {
                    _generalCriteria.Add((CriteriaType)i, pendingGeneralCriteria[i]!);
                }

                continue;
            }

            for (var j = 0; j < _beatmapIds.Length; j++)
            {
                _specificCriteriaForBeatmap[j].Add(
                    (CriteriaType)i,
                    pendingSpecificCriteria[i]![(j * passTypeCount)..((j + 1) * passTypeCount)]);
            }
        }
    }

    public int CurrentStage => Math.Min(_currentStage + 1, _beatmapIds.Length);
    public int CurrentPlaylistId => _currentStage < _playlistIds.Length ? _playlistIds[_currentStage] : 0;
    public int CurrentBeatmapId => _currentStage < _beatmapIds.Length ? _beatmapIds[_currentStage] : 0;
    public int CurrentBeatmapLength => _currentStage < _totalLengths.Length ? _totalLengths[_currentStage] : 0;
    public bool IsEnded => _currentStage >= _beatmapIds.Length;
    public int PassLevel { get; private set; } = (int)ExamResult.Count;

    public bool Judge(BeatmapResultQuery beatmapResultQuery)
    {
        if (IsEnded)
        {
            throw new InvalidOperationException("Exam has ended.");
        }

        var result = _specificCriteriaForBeatmap[_currentStage].Check(
            beatmapResultQuery.GreatCount,
            beatmapResultQuery.OkCount,
            beatmapResultQuery.MissCount,
            beatmapResultQuery.LargeBonusCount,
            beatmapResultQuery.MaxCombo,
            beatmapResultQuery.HitCount);

        if (result == ExamResult.Failed)
        {
            return false;
        }

        PassLevel = Math.Min(PassLevel, (int)result);
        _currentStage++;
        _totalGreatCount += beatmapResultQuery.GreatCount;
        _totalOkCount += beatmapResultQuery.OkCount;
        _totalMissCount += beatmapResultQuery.MissCount;
        _totalLargeBonusCount += beatmapResultQuery.LargeBonusCount;
        _totalMaxCombo += beatmapResultQuery.MaxCombo;
        _totalHitCount += beatmapResultQuery.HitCount;

        if (!IsEnded)
        {
            return true;
        }

        result = _generalCriteria.Check(
            _totalGreatCount,
            _totalOkCount,
            _totalMissCount,
            _totalLargeBonusCount,
            _totalMaxCombo,
            _totalHitCount);

        if (result == ExamResult.Failed)
        {
            return false;
        }

        PassLevel = Math.Min(PassLevel, (int)result);
        return true;
    }
}
