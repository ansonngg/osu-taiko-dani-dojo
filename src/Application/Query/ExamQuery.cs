namespace OsuTaikoDaniDojo.Application.Query;

public class ExamQuery
{
    public int[] BeatmapIds { get; init; } = [];
    public int[]? SpecificGreatCounts { get; init; }
    public int[]? SpecificOkCounts { get; init; }
    public int[]? SpecificMissCounts { get; init; }
    public int[]? SpecificLargeBonusCounts { get; init; }
    public int[]? SpecificMaxCombos { get; init; }
    public int[]? SpecificHitCounts { get; init; }
    public int GeneralGreatCount { get; init; }
    public int GeneralOkCount { get; init; }
    public int GeneralMissCount { get; init; }
    public int GeneralLargeBonusCount { get; init; }
    public int GeneralMaxCombo { get; init; }
    public int GeneralHitCount { get; init; }
}
