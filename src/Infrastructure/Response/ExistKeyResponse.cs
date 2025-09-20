using System.Text.Json.Serialization;

namespace Infrastructure.Response;

public class ExistKeyResponse
{
    [JsonPropertyName("result")]
    public int Result { get; init; }
}
