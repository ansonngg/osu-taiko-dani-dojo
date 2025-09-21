namespace OsuTaikoDaniDojo.Infrastructure.Utility;

public static class UrlHelper
{
    public static string ParameterizedWith(this string uri, Dictionary<string, string> queryParams)
    {
        var query = string.Join("&", queryParams.Select(kvp => $"{kvp.Key}={Uri.EscapeDataString(kvp.Value)}"));
        return $"{uri}?{query}";
    }
}
