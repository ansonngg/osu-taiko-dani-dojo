using System.Text.Json.Serialization;

namespace OsuTaikoDaniDojo.Presentation.Response;

public class AuthorizeUrlResponse
{
    [JsonPropertyName("url")]
    public string Url { get; init; } = string.Empty;
}
