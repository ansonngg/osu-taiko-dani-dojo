using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace OsuTaikoDaniDojo.Infrastructure.Model;

[Table("exam")]
public class Exam : BaseModel
{
    [PrimaryKey("id")]
    public int Id { get; init; }

    [Column("grade")]
    public int Grade { get; init; }

    [Column("beatmap_ids")]
    public int[] BeatmapIds { get; init; } = [];

    [Column("specific_great_counts")]
    public int[]? SpecificGreatCounts { get; init; }

    [Column("specific_ok_counts")]
    public int[]? SpecificOkCounts { get; init; }

    [Column("specific_miss_counts")]
    public int[]? SpecificMissCounts { get; init; }

    [Column("specific_large_bonus_counts")]
    public int[]? SpecificLargeBonusCounts { get; init; }

    [Column("specific_max_combos")]
    public int[]? SpecificMaxCombos { get; init; }

    [Column("specific_hit_counts")]
    public int[]? SpecificHitCounts { get; init; }

    [Column("general_great_count")]
    public int[]? GeneralGreatCounts { get; init; }

    [Column("general_ok_count")]
    public int[]? GeneralOkCounts { get; init; }

    [Column("general_miss_count")]
    public int[]? GeneralMissCounts { get; init; }

    [Column("general_large_bonus_count")]
    public int[]? GeneralLargeBonusCounts { get; init; }

    [Column("general_max_combo")]
    public int[]? GeneralMaxCombos { get; init; }

    [Column("general_hit_count")]
    public int[]? GeneralHitCounts { get; init; }
}
