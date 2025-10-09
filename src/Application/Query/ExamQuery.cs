namespace OsuTaikoDaniDojo.Application.Query;

public class ExamQuery
{
    public int Grade { get; init; }
    public int[] BeatmapIds { get; init; } = [];
    public int[]? SpecificGreatCounts { get; init; }
    public int[]? SpecificOkCounts { get; init; }
    public int[]? SpecificMissCounts { get; init; }
    public int[]? SpecificLargeBonusCounts { get; init; }
    public int[]? SpecificMaxCombos { get; init; }
    public int[]? SpecificHitCounts { get; init; }
    public int[]? GeneralGreatCounts { get; init; }
    public int[]? GeneralOkCounts { get; init; }
    public int[]? GeneralMissCounts { get; init; }
    public int[]? GeneralLargeBonusCounts { get; init; }
    public int[]? GeneralMaxCombos { get; init; }
    public int[]? GeneralHitCounts { get; init; }
}
