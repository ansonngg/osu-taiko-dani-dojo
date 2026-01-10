namespace OsuTaikoDaniDojo.Application.Options;

public class LoginSessionOptions
{
    public int RedisExpiryInDay { get; init; }
    public int MemoryCacheExpiryInMinute { get; init; }
    public int CookieExpiryInDay { get; init; }
}
