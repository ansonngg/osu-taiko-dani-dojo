namespace OsuTaikoDaniDojo.Web.Utility;

public static class ResponseCookiesExtension
{
    public static void Append(this IResponseCookies cookies, string key, string value, DateTimeOffset expires)
    {
        cookies.Append(
            key,
            value,
            new CookieOptions { HttpOnly = true, Secure = true, SameSite = SameSiteMode.Lax, Expires = expires });
    }
}
