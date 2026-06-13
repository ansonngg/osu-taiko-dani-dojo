using System.Text.Json.Serialization;

namespace OsuDojo.Web.Request;

public class LoginRequest
{
    [JsonPropertyName("code")]
    public string Code { get; init; } = string.Empty;
}
