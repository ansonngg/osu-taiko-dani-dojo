using OsuTaikoDaniDojo.Application.System;

namespace OsuTaikoDaniDojo.Web.Context;

public class ExamSessionContext
{
    public int ExamSessionId { get; init; }
    public int RoomId { get; init; }
    public DateTime StartedAt { get; init; }
    public required ExamTracker ExamTracker { get; init; }
}
