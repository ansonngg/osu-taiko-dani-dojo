using OsuTaikoDaniDojo.Application.System;
using OsuTaikoDaniDojo.Web.Utility;

namespace OsuTaikoDaniDojo.Web.Context;

public class ExamSessionContext
{
    public int ExamSessionId { get; init; }
    public int UserId { get; init; }
    public int RoomId { get; init; }
    public required ExamTracker ExamTracker { get; init; }
    public ExamSessionStatus Status { get; set; } = ExamSessionStatus.Waiting;
}
