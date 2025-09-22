using System.Text.Json.Serialization;

namespace OsuTaikoDaniDojo.Web.Response;

public class AuthorizeUrlResponse
{
    [JsonPropertyName("url")]
    public string Url { get; init; } = string.Empty;
}
