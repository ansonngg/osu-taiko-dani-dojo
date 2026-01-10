namespace OsuTaikoDaniDojo.Web.Context;

public class LoginSessionContext
{
    public int UserId { get; init; }
    public int OsuId { get; init; }
    public string Role { get; init; } = "User";
    public string AccessToken { get; init; } = string.Empty;
    public string RefreshToken { get; init; } = string.Empty;
    public DateTime ExpiresAt { get; init; }
}
