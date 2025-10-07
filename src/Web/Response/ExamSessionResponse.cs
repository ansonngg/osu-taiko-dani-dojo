using System.Text.Json.Serialization;

namespace OsuTaikoDaniDojo.Web.Response;

public class ExamSessionResponse
{
    [JsonPropertyName("id")]
    public int? Id { get; init; }

    [JsonPropertyName("is_room_active")]
    public bool IsRoomActive { get; init; }

    [JsonPropertyName("is_playlist_correct")]
    public bool[] IsPlaylistCorrect { get; init; } = [];
}
