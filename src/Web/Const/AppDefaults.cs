namespace OsuTaikoDaniDojo.Web.Const;

public static class AppDefaults
{
    public const string AuthenticationScheme = "OsuSession";
    public const string SessionIdCookieName = "otdd_session";

    public static readonly TimeSpan OsuPollingInterval = TimeSpan.FromSeconds(1);
    public static readonly TimeSpan OsuPollingDuration = TimeSpan.FromSeconds(30);
}
