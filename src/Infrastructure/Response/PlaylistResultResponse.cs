using System.Text.Json.Serialization;

namespace OsuTaikoDaniDojo.Infrastructure.Response;

public class PlaylistResultResponse
{
    [JsonPropertyName("user_score")]
    public Score? UserScore { get; init; }
}

public class Score
{
    [JsonPropertyName("statistics")]
    public Statistics Statistics { get; init; } = new();

    [JsonPropertyName("max_combo")]
    public int MaxCombo { get; init; }
}

public class Statistics
{
    [JsonPropertyName("great")]
    public int Great { get; init; }

    [JsonPropertyName("ok")]
    public int Ok { get; init; }

    [JsonPropertyName("miss")]
    public int Miss { get; init; }

    [JsonPropertyName("large_bonus")]
    public int LargeBonus { get; init; }

    [JsonPropertyName("small_bonus")]
    public int SmallBonus { get; init; }
}
