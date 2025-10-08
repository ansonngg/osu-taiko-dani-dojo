using OsuTaikoDaniDojo.Application.Query;

namespace OsuTaikoDaniDojo.Application.Interface;

public interface IOsuMultiplayerRoomService
{
    Task<MultiplayerRoomQuery?> GetMostRecentActiveRoomAsync(int userId, string? accessToken = null);
    Task<RoomPlaylistQuery> GetRoomPlaylistAsync(int roomId);
    Task<BeatmapResultQuery?> GetBeatmapResultAsync(int roomId, int playlistId, string? accessToken = null);
}
