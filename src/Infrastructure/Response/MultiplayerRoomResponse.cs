using System.Text.Json.Serialization;

namespace OsuTaikoDaniDojo.Infrastructure.Response;

public class MultiplayerRoomResponse
{
    [JsonPropertyName("id")]
    public int Id { get; init; }

    [JsonPropertyName("active")]
    public bool Active { get; init; }

    [JsonPropertyName("current_playlist_item")]
    public PlaylistResponse CurrentPlaylistItem { get; init; } = new();

    [JsonPropertyName("playlist")]
    public PlaylistResponse[] Playlist { get; init; } = [];
}

public class PlaylistResponse
{
    [JsonPropertyName("id")]
    public int Id { get; init; }

    [JsonPropertyName("played_at")]
    public DateTime? PlayedAt { get; init; }

    [JsonPropertyName("beatmap")]
    public BeatmapResponse Beatmap { get; init; } = new();
}

public class BeatmapResponse
{
    [JsonPropertyName("id")]
    public int Id { get; init; }

    [JsonPropertyName("total_length")]
    public int TotalLength { get; init; }
}
