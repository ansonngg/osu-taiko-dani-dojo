using System.Text.Json.Serialization;

namespace OsuDojo.Infrastructure.Response;

public class ExistKeyResponse
{
    [JsonPropertyName("result")]
    public int Result { get; init; }
}
