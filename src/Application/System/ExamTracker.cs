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
    private readonly int[] _greatCounts;
    private readonly int[] _okCounts;
    private readonly int[] _missCounts;
    private readonly int[] _largeBonusCounts;
    private readonly int[] _maxCombos;
    private readonly int[] _hitCounts;
    private int _currentStage;

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
        _greatCounts = new int[_beatmapIds.Length];
        _okCounts = new int[_beatmapIds.Length];
        _missCounts = new int[_beatmapIds.Length];
        _largeBonusCounts = new int[_beatmapIds.Length];
        _maxCombos = new int[_beatmapIds.Length];
        _hitCounts = new int[_beatmapIds.Length];

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
    public int[] GreatCounts => (int[])_greatCounts.Clone();
    public int[] OkCounts => (int[])_okCounts.Clone();
    public int[] MissCounts => (int[])_missCounts.Clone();
    public int[] LargeBonusCounts => (int[])_largeBonusCounts.Clone();
    public int[] MaxCombos => (int[])_maxCombos.Clone();
    public int[] HitCounts => (int[])_hitCounts.Clone();
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
        _greatCounts[_currentStage] = beatmapResultQuery.GreatCount;
        _okCounts[_currentStage] = beatmapResultQuery.OkCount;
        _missCounts[_currentStage] = beatmapResultQuery.MissCount;
        _largeBonusCounts[_currentStage] = beatmapResultQuery.LargeBonusCount;
        _maxCombos[_currentStage] = beatmapResultQuery.MaxCombo;
        _hitCounts[_currentStage] = beatmapResultQuery.HitCount;
        _currentStage++;

        if (!IsEnded)
        {
            return true;
        }

        result = _generalCriteria.Check(
            _greatCounts.Sum(),
            _okCounts.Sum(),
            _missCounts.Sum(),
            _largeBonusCounts.Sum(),
            _maxCombos.Sum(),
            _hitCounts.Sum());

        if (result == ExamResult.Failed)
        {
            return false;
        }

        PassLevel = Math.Min(PassLevel, (int)result);
        return true;
    }
}
