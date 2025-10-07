using OsuTaikoDaniDojo.Application.Query;

namespace OsuTaikoDaniDojo.Web.Context;

public class ExamSessionContext
{
    public int ExamSessionId { get; init; }
    public int LastReachedStage { get; init; } = 1;
    public DateTime StartedAt { get; init; }
    public int RoomId { get; init; }
    public int[] PlaylistIds { get; init; } = [];
    public int[] TotalLengths { get; init; } = [];
    public ExamQuery ExamQuery { get; init; } = new();
    public int TotalGreatCount { get; init; }
    public int TotalOkCount { get; init; }
    public int TotalMissCount { get; init; }
    public int TotalLargeBonusCount { get; init; }
    public int TotalMaxCombo { get; init; }
    public int TotalHitCount { get; init; }
}
