using System.Text.Json.Serialization;

namespace OsuTaikoDaniDojo.Infrastructure.Response;

public class UserDataResponse
{
    [JsonPropertyName("id")]
    public int Id { get; init; }
}
