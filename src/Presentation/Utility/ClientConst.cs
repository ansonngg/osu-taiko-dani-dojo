namespace OsuTaikoDaniDojo.Presentation.Utility;

public static class ClientConst
{
    public const string SessionIdCookieName = "otdd_session";

    public static readonly int SessionIdCookieExpiryInSecond = (int)TimeSpan.FromDays(14).TotalSeconds;
}
