using System.Text.Json.Serialization;

namespace OsuTaikoDaniDojo.Presentation.Request;

public class LoginRequest
{
    [JsonPropertyName("code")]
    public string Code { get; init; } = string.Empty;
}
