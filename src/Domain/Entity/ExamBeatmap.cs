using OsuTaikoDaniDojo.Domain.Utility;

namespace OsuTaikoDaniDojo.Domain.Entity;

public class ExamBeatmap
{
    private readonly ExamCriteria[] _criteriaList = new ExamCriteria[(int)PassType.Count];

    public int Id { get; }
    public int PlaylistId { get; }
    public int Length { get; }
    public int GreatCount { get; private set; }
    public int OkCount { get; private set; }
    public int MissCount { get; private set; }
    public int LargeBonusCount { get; private set; }
    public int MaxCombo { get; private set; }
    public int HitCount { get; private set; }

    public ExamBeatmap(int beatmapId, int playlistId, int length)
    {
        Id = beatmapId;
        PlaylistId = playlistId;
        Length = length;
        _criteriaList.Reset();
    }

    public void AddCriteria(CriteriaType type, int[] thresholds)
    {
        for (var i = 0; i < (int)PassType.Count; i++)
        {
            _criteriaList[i].Add(type, thresholds[i]);
        }
    }

    public PassType CheckCriteria(
        int greatCount,
        int okCount,
        int missCount,
        int largeBonusCount,
        int maxCombo,
        int hitCount,
        PassType highestPassType)
    {
        GreatCount = greatCount;
        OkCount = okCount;
        MissCount = missCount;
        LargeBonusCount = largeBonusCount;
        MaxCombo = maxCombo;
        HitCount = hitCount;

        return _criteriaList.Check(
            GreatCount,
            OkCount,
            MissCount,
            LargeBonusCount,
            MaxCombo,
            HitCount,
            highestPassType);
    }
}
