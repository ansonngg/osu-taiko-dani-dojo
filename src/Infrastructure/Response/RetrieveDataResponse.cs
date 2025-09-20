using System.Text.Json.Serialization;

namespace Infrastructure.Response;

public class RetrieveDataResponse
{
    [JsonPropertyName("result")]
    public string Result { get; init; } = string.Empty;
}
