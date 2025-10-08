using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace OsuTaikoDaniDojo.Infrastructure.Model;

[Table("user")]
public class User : BaseModel
{
    [PrimaryKey("id")]
    public int Id { get; init; }

    [Column("osu_id")]
    public int OsuId { get; init; }

    [Column("highest_grade_certificate_id")]
    public int HighestGradeCertificateId { get; init; }

    [Column("is_grade_valid")]
    public bool IsGradeValid { get; init; }

    [Column("role")]
    public string Role { get; init; } = "user";
}
