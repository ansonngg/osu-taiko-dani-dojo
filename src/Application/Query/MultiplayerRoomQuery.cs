namespace OsuTaikoDaniDojo.Application.Query;

public class MultiplayerRoomQuery
{
    public int RoomId { get; init; }
    public bool IsActive { get; init; }
    public int CurrentPlaylistId { get; init; }
    public DateTime? LastPlayedAt { get; init; }
    public int CurrentBeatmapId { get; init; }
}
