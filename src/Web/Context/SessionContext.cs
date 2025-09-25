namespace OsuTaikoDaniDojo.Web.Context;

public class SessionContext
{
    public int UserId { get; init; }
    public string AccessToken { get; init; } = string.Empty;
    public string RefreshToken { get; init; } = string.Empty;
    public DateTime ExpiresAt { get; init; }
}
