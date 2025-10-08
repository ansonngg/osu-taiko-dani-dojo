using OsuTaikoDaniDojo.Application.Query;
using OsuTaikoDaniDojo.Domain.Entity;
using OsuTaikoDaniDojo.Domain.Utility;

namespace OsuTaikoDaniDojo.Application.System;

public class ExamTracker
{
    private readonly ExamBeatmap[] _examBeatmaps;
    private readonly ExamCriteria[] _generalCriteriaList = new ExamCriteria[(int)PassType.Count];
    private int _currentStage;
    private PassType _passType = PassType.Count - 1;

    public ExamTracker(ExamQuery examQuery, int[] playlistIds, int[] totalLengths)
    {
        _examBeatmaps = new ExamBeatmap[examQuery.BeatmapIds.Length];

        for (var i = 0; i < _examBeatmaps.Length; i++)
        {
            _examBeatmaps[i] = new ExamBeatmap(examQuery.BeatmapIds[i], playlistIds[i], totalLengths[i]);
        }

        _generalCriteriaList.Initialize();

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

        for (var i = 0; i < pendingSpecificCriteria.Length; i++)
        {
            if (pendingSpecificCriteria[i] == null
                || pendingSpecificCriteria[i]!.Length != (int)PassType.Count * _examBeatmaps.Length)
            {
                if (pendingGeneralCriteria[i] is { Length: (int)PassType.Count })
                {
                    for (var j = 0; j < (int)PassType.Count; j++)
                    {
                        _generalCriteriaList[j].Add((CriteriaType)i, pendingGeneralCriteria[i]![j]);
                    }
                }

                continue;
            }

            for (var j = 0; j < _examBeatmaps.Length; j++)
            {
                _examBeatmaps[j].AddCriteria(
                    (CriteriaType)i,
                    pendingSpecificCriteria[i]![(j * (int)PassType.Count)..((j + 1) * (int)PassType.Count)]);
            }
        }
    }

    public int CurrentStage => Math.Min(_currentStage + 1, _examBeatmaps.Length);
    public int CurrentPlaylistId => !IsEnded ? _examBeatmaps[_currentStage].PlaylistId : 0;
    public int CurrentBeatmapId => !IsEnded ? _examBeatmaps[_currentStage].Id : 0;
    public int CurrentBeatmapLength => !IsEnded ? _examBeatmaps[_currentStage].Length : 0;
    public bool IsEnded => _currentStage >= _examBeatmaps.Length;
    public int[] GreatCounts => _examBeatmaps.Select(x => x.GreatCount).ToArray();
    public int[] OkCounts => _examBeatmaps.Select(x => x.OkCount).ToArray();
    public int[] MissCounts => _examBeatmaps.Select(x => x.MissCount).ToArray();
    public int[] LargeBonusCounts => _examBeatmaps.Select(x => x.LargeBonusCount).ToArray();
    public int[] MaxCombos => _examBeatmaps.Select(x => x.MaxCombo).ToArray();
    public int[] HitCounts => _examBeatmaps.Select(x => x.HitCount).ToArray();
    public int PassLevel => (int)_passType;

    public bool Judge(BeatmapResultQuery beatmapResultQuery)
    {
        if (IsEnded)
        {
            throw new InvalidOperationException("Exam has ended.");
        }

        _passType = _examBeatmaps[_currentStage].CheckCriteria(
            beatmapResultQuery.GreatCount,
            beatmapResultQuery.OkCount,
            beatmapResultQuery.MissCount,
            beatmapResultQuery.LargeBonusCount,
            beatmapResultQuery.MaxCombo,
            beatmapResultQuery.HitCount,
            _passType);

        if (_passType == PassType.Invalid)
        {
            return false;
        }

        _currentStage++;

        if (!IsEnded)
        {
            return true;
        }

        _passType = _generalCriteriaList.Check(
            _examBeatmaps.Select(x => x.GreatCount).Sum(),
            _examBeatmaps.Select(x => x.OkCount).Sum(),
            _examBeatmaps.Select(x => x.MissCount).Sum(),
            _examBeatmaps.Select(x => x.LargeBonusCount).Sum(),
            _examBeatmaps.Select(x => x.MaxCombo).Sum(),
            _examBeatmaps.Select(x => x.HitCount).Sum(),
            _passType);

        return _passType != PassType.Invalid;
    }
}
