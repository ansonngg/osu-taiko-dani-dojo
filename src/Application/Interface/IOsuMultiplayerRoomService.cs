using OsuTaikoDaniDojo.Application.Query;

namespace OsuTaikoDaniDojo.Application.Interface;

public interface IOsuMultiplayerRoomService
{
    void SetAuthenticationHeader(string accessToken);
    Task<MultiplayerRoomQuery?> GetMostRecentActiveRoomAsync();
    Task<RoomPlaylistQuery> GetRoomPlaylistAsync(int roomId);
}
