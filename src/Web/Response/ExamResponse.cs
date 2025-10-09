using System.Text.Json.Serialization;

namespace OsuTaikoDaniDojo.Web.Response;

public class ExamResponse
{
    [JsonPropertyName("grade")]
    public int Grade { get; init; }

    [JsonPropertyName("beatmap_ids")]
    public int[] BeatmapIds { get; init; } = [];

    [JsonPropertyName("specific_great_counts")]
    public int[]? SpecificGreatCounts { get; init; }

    [JsonPropertyName("specific_ok_counts")]
    public int[]? SpecificOkCounts { get; init; }

    [JsonPropertyName("specific_miss_counts")]
    public int[]? SpecificMissCounts { get; init; }

    [JsonPropertyName("specific_large_bonus_counts")]
    public int[]? SpecificLargeBonusCounts { get; init; }

    [JsonPropertyName("specific_max_combos")]
    public int[]? SpecificMaxCombos { get; init; }

    [JsonPropertyName("specific_hit_counts")]
    public int[]? SpecificHitCounts { get; init; }

    [JsonPropertyName("general_great_counts")]
    public int[]? GeneralGreatCounts { get; init; }

    [JsonPropertyName("general_ok_counts")]
    public int[]? GeneralOkCounts { get; init; }

    [JsonPropertyName("general_miss_counts")]
    public int[]? GeneralMissCounts { get; init; }

    [JsonPropertyName("general_large_bonus_counts")]
    public int[]? GeneralLargeBonusCounts { get; init; }

    [JsonPropertyName("general_max_combos")]
    public int[]? GeneralMaxCombos { get; init; }

    [JsonPropertyName("general_hit_counts")]
    public int[]? GeneralHitCounts { get; init; }
}
