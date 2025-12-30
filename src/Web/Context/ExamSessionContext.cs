using OsuTaikoDaniDojo.Application.Exam;
using OsuTaikoDaniDojo.Web.Const;

namespace OsuTaikoDaniDojo.Web.Context;

public class ExamSessionContext
{
    private ExamSessionStatus _status = ExamSessionStatus.Waiting;

    public int ExamSessionId { get; init; }
    public int UserId { get; init; }
    public int OsuId { get; init; }
    public int Grade { get; init; }
    public int RoomId { get; init; }
    public required ExamTracker ExamTracker { get; init; }

    public ExamSessionStatus Status
    {
        get => _status;
        set
        {
            _status = value;
            LastUpdatedAt = DateTime.UtcNow;
        }
    }

    public DateTime LastUpdatedAt { get; set; } = DateTime.UtcNow;
}
