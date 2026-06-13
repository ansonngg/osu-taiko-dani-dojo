using System.Text.Json.Serialization;

namespace OsuDojo.Infrastructure.Response;

public class TokenResponse
{
    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; init; }

    [JsonPropertyName("access_token")]
    public string AccessToken { get; init; } = string.Empty;

    [JsonPropertyName("refresh_token")]
    public string RefreshToken { get; init; } = string.Empty;
}
