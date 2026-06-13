using System.Text.Json.Serialization;

namespace OsuDojo.Infrastructure.Response;

public class RetrieveDataResponse
{
    [JsonPropertyName("result")]
    public string Result { get; init; } = string.Empty;
}
