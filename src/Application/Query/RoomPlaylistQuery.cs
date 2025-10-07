namespace OsuTaikoDaniDojo.Application.Query;

public class RoomPlaylistQuery
{
    public int[] PlaylistIds { get; init; } = [];
    public int[] BeatmapIds { get; init; } = [];
    public int[] TotalLengths { get; init; } = [];
}
