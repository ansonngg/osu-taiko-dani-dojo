namespace OsuTaikoDaniDojo.Application.Options;

public class LoginSessionOptions
{
    public int SessionExpiryInDay { get; init; }
    public int CookieExpiryInDay { get; init; }
    public int MaxSessionCountPerUser { get; init; }
}
