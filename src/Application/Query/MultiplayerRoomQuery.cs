namespace OsuTaikoDaniDojo.Application.Query;

public class MultiplayerRoomQuery
{
    public int RoomId { get; init; }
    public string Status { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public int CurrentPlaylistId { get; init; }
    public int CurrentBeatmapId { get; init; }
    public int ActivePlaylistCount { get; init; }
}
