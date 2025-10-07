using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OsuTaikoDaniDojo.Application.Interface;
using OsuTaikoDaniDojo.Application.Options;
using OsuTaikoDaniDojo.Application.Query;
using OsuTaikoDaniDojo.Infrastructure.Response;
using OsuTaikoDaniDojo.Infrastructure.Utility;

namespace OsuTaikoDaniDojo.Infrastructure.Service;

public class OsuAuthService : IOsuAuthService
{
    private readonly HttpClient _httpClient;
    private readonly OsuOptions _osuOptions;
    private readonly ILogger<OsuAuthService> _logger;

    public OsuAuthService(HttpClient httpClient, IOptions<OsuOptions> osuOptions, ILogger<OsuAuthService> logger)
    {
        _httpClient = httpClient;
        _osuOptions = osuOptions.Value;
        _logger = logger;
        _httpClient.BaseAddress = new Uri("https://osu.ppy.sh");
    }

    public string GetAuthorizeUrl()
    {
        if (_httpClient.BaseAddress == null)
        {
            throw new NullReferenceException("Base address is null.");
        }

        var queryParams = new Dictionary<string, string>
        {
            ["client_id"] = _osuOptions.ClientId,
            ["redirect_uri"] = _osuOptions.RedirectUri,
            ["response_type"] = "code",
            ["scope"] = _osuOptions.Scope
        };

        var authorizeUrl = new Uri(_httpClient.BaseAddress, "oauth/authorize").AbsoluteUri;
        return authorizeUrl.ParameterizedWith(queryParams);
    }

    public async Task<TokenQuery> ExchangeTokenAsync(string code)
    {
        var bodyParams = new
        {
            client_id = _osuOptions.ClientId,
            client_secret = _osuOptions.ClientSecret,
            code,
            grant_type = "authorization_code",
            redirect_uri = _osuOptions.RedirectUri
        };

        var response = await _httpClient.PostAsJsonAsync("oauth/token", bodyParams);
        response.EnsureSuccessStatusCode();
        return _ConstructTokenQueryAsync(await response.Content.ReadFromJsonAsync<TokenResponse>());
    }

    public async Task<TokenQuery?> RefreshTokenAsync(string refreshToken)
    {
        var bodyParams = new
        {
            client_id = _osuOptions.ClientId,
            client_secret = _osuOptions.ClientSecret,
            grant_type = "refresh_token",
            refresh_token = refreshToken
        };

        var response = await _httpClient.PostAsJsonAsync("oauth/token", bodyParams);

        try
        {
            response.EnsureSuccessStatusCode();
        }
        catch
        {
            _logger.LogWarning("{StatusCode}", response.StatusCode.ToString());
            return null;
        }

        return _ConstructTokenQueryAsync(await response.Content.ReadFromJsonAsync<TokenResponse>());
    }

    public async Task<int> GetUserIdAsync(string accessToken)
    {
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var response = await _httpClient.GetAsync("api/v2/me");
        response.EnsureSuccessStatusCode();
        var userDataResponse = await response.Content.ReadFromJsonAsync<UserDataResponse>();
        return userDataResponse?.Id ?? throw new NullReferenceException("User data response is null.");
    }

    private TokenQuery _ConstructTokenQueryAsync(TokenResponse? tokenResponse)
    {
        if (tokenResponse == null)
        {
            throw new NullReferenceException("Token response is null.");
        }

        return new TokenQuery
        {
            AccessToken = tokenResponse.AccessToken,
            RefreshToken = tokenResponse.RefreshToken,
            ExpiresAt = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn)
        };
    }
}
