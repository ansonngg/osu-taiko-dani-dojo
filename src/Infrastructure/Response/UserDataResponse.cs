using System.Text.Json.Serialization;

namespace Infrastructure.Response;

public class UserDataResponse
{
    [JsonPropertyName("id")]
    public int Id { get; init; }
}
