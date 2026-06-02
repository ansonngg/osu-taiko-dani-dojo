using OsuTaikoDaniDojo.Application.Const;
using OsuTaikoDaniDojo.Application.Exam;

namespace OsuTaikoDaniDojo.Application.Context;

public class ExamSessionContext
{
    public int ExamSessionId { get; init; }
    public int UserId { get; init; }
    public int OsuId { get; init; }
    public int Grade { get; init; }
    public int RoomId { get; init; }
    public required ExamTracker ExamTracker { get; init; }

    public ExamSessionStatus Status
    {
        get;
        set
        {
            field = value;
            LastUpdatedAt = DateTime.UtcNow;
        }
    } = ExamSessionStatus.Waiting;

    public DateTime LastUpdatedAt { get; private set; } = DateTime.UtcNow;
}
