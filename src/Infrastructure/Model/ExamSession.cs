using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace OsuTaikoDaniDojo.Infrastructure.Model;

[Table("exam_session")]
public class ExamSession : BaseModel
{
    [PrimaryKey("id")]
    public int Id { get; init; }

    [Column("osu_id")]
    public int OsuId { get; init; }

    [Column("grade")]
    public int Grade { get; init; }

    [Column("last_reached_stage")]
    public int LastReachedStage { get; init; } = 1;

    [Column("started_at")]
    public DateTime StartedAt { get; init; } = DateTime.UtcNow;

    [Column("ended_at")]
    public DateTime? EndedAt { get; init; }

    [Column("status")]
    public string Status { get; init; } = "in_progress";
}
