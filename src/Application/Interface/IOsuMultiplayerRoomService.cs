using OsuTaikoDaniDojo.Application.Query;

namespace OsuTaikoDaniDojo.Application.Interface;

public interface IOsuMultiplayerRoomService
{
    void SetAuthenticationHeader(string accessToken);
    Task<MultiplayerRoomQuery?> GetMostRecentActiveRoomAsync(int userId);
    Task<RoomPlaylistQuery> GetRoomPlaylistAsync(int roomId);
    Task<BeatmapResultQuery?> GetBeatmapResultAsync(int roomId, int playlistId);
}
