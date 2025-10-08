namespace OsuTaikoDaniDojo.Application.Query;

public class BeatmapResultQuery
{
    public int GreatCount { get; init; }
    public int OkCount { get; init; }
    public int MissCount { get; init; }
    public int LargeBonusCount { get; init; }
    public int MaxCombo { get; init; }
    public int HitCount { get; init; }
    public bool HasMods { get; init; }
}
