using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace OsuTaikoDaniDojo.Infrastructure.Model;

[Table("grade_certificate")]
public class GradeCertificate : BaseModel
{
    [PrimaryKey("id")]
    public int Id { get; init; }

    [Column("user_id")]
    public int UserId { get; init; }

    [Column("grade")]
    public int Grade { get; init; }

    [Column("pass_level")]
    public int PassLevel { get; init; }

    [Column("achieved_at")]
    public DateTime AchievedAt { get; init; } = DateTime.UtcNow;

    [Column("great_counts")]
    public int[] GreatCounts { get; init; } = [];

    [Column("ok_counts")]
    public int[] OkCounts { get; init; } = [];

    [Column("miss_counts")]
    public int[] MissCounts { get; init; } = [];

    [Column("large_bonus_counts")]
    public int[] LargeBonusCounts { get; init; } = [];

    [Column("max_combos")]
    public int[] MaxCombos { get; init; } = [];

    [Column("hit_counts")]
    public int[] HitCounts { get; init; } = [];

    [Column("exam_session_id")]
    public int ExamSessionId { get; init; }

    [Column("is_valid")]
    public bool IsValid { get; init; } = true;
}
