using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.WebUtilities;
using OsuTaikoDaniDojo.Application.Interface;
using OsuTaikoDaniDojo.Application.Query;
using OsuTaikoDaniDojo.Infrastructure.Response;

namespace OsuTaikoDaniDojo.Infrastructure.Service;

public class OsuMultiplayerRoomService : IOsuMultiplayerRoomService
{
    private readonly HttpClient _httpClient;

    public OsuMultiplayerRoomService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri("https://osu.ppy.sh/api/v2/rooms/");
    }

    public void SetAuthenticationHeader(string accessToken)
    {
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
    }

    public async Task<MultiplayerRoomQuery?> GetMostRecentActiveRoomAsync()
    {
        var queryParams = new Dictionary<string, string?>
        {
            ["limit"] = "1",
            ["mode"] = "participated",
            ["sort"] = "created",
            ["type_group"] = "realtime"
        };

        var response = await _httpClient.GetAsync(QueryHelpers.AddQueryString("", queryParams));
        response.EnsureSuccessStatusCode();
        var multiplayerRoomResponses = await response.Content.ReadFromJsonAsync<MultiplayerRoomResponse[]>();

        if (multiplayerRoomResponses == null)
        {
            throw new NullReferenceException("Multiplayer room response array is null.");
        }

        return multiplayerRoomResponses[0].Active
            ? new MultiplayerRoomQuery { RoomId = multiplayerRoomResponses[0].Id }
            : null;
    }

    public async Task<RoomPlaylistQuery> GetRoomPlaylistAsync(int roomId)
    {
        var response = await _httpClient.GetAsync($"{roomId}");
        response.EnsureSuccessStatusCode();
        var multiplayerRoomResponse = await response.Content.ReadFromJsonAsync<MultiplayerRoomResponse>();

        if (multiplayerRoomResponse == null)
        {
            throw new NullReferenceException("Multiplayer room response is null.");
        }

        var playlistCount = multiplayerRoomResponse.Playlist.Length;
        var playlistIds = new int[playlistCount];
        var beatmapIds = new int[playlistCount];
        var totalLengths = new int[playlistCount];

        for (var i = 0; i < playlistCount; i++)
        {
            var playlist = multiplayerRoomResponse.Playlist[i];
            playlistIds[i] = playlist.Id;
            beatmapIds[i] = playlist.Beatmap.Id;
            totalLengths[i] = playlist.Beatmap.TotalLength;
        }

        return new RoomPlaylistQuery
        {
            PlaylistIds = playlistIds,
            BeatmapIds = beatmapIds,
            TotalLengths = totalLengths
        };
    }
}
