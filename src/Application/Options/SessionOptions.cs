namespace OsuTaikoDaniDojo.Application.Options;

public class SessionOptions
{
    public int RedisExpiryInDay { get; init; }
    public int MemoryCacheExpiryInMinute { get; init; }
    public int CookieExpiryInDay { get; init; }
}
