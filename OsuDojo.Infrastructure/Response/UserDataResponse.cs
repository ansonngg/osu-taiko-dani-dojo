using System.Text.Json.Serialization;

namespace OsuDojo.Infrastructure.Response;

public class UserDataResponse
{
    [JsonPropertyName("id")]
    public int Id { get; init; }
}
