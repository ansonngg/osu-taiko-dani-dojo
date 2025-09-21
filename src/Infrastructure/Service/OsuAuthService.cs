using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using OsuTaikoDaniDojo.Application.Interface;
using OsuTaikoDaniDojo.Application.Model;
using OsuTaikoDaniDojo.Application.Options;
using OsuTaikoDaniDojo.Application.Utility;
using OsuTaikoDaniDojo.Infrastructure.Response;
using OsuTaikoDaniDojo.Infrastructure.Utility;

namespace OsuTaikoDaniDojo.Infrastructure.Service;

public class OsuAuthService : IOsuAuthService
{
    private readonly HttpClient _httpClient;
    private readonly OsuOptions _options;

    public OsuAuthService(HttpClient httpClient, IOptions<OsuOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _httpClient.BaseAddress = new Uri("https://osu.ppy.sh");
    }

    public string GetAuthorizeUrl()
    {
        if (_httpClient.BaseAddress == null)
        {
            throw this.ExceptionSince("Base address is null");
        }

        var queryParams = new Dictionary<string, string>
        {
            ["client_id"] = _options.ClientId,
            ["redirect_uri"] = _options.RedirectUri,
            ["response_type"] = "code",
            ["scope"] = _options.Scope
        };

        var authorizeUrl = new Uri(_httpClient.BaseAddress + "oauth/authorize").AbsoluteUri;
        return authorizeUrl.ParameterizedWith(queryParams);
    }

    public async Task<UserToken> ExchangeTokenAsync(string code)
    {
        var bodyParams = new
        {
            client_id = _options.ClientId,
            client_secret = _options.ClientSecret,
            code,
            grant_type = "authorization_code",
            redirect_uri = _options.RedirectUri
        };

        var response = await _httpClient.PostAsJsonAsync("oauth/token", bodyParams);
        response.EnsureSuccessStatusCode();
        return await _ConstructUserTokenAsync(response);
    }

    public async Task<UserToken?> RefreshTokenAsync(string refreshToken)
    {
        var bodyParams = new
        {
            client_id = _options.ClientId,
            client_secret = _options.ClientSecret,
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
            this.Log(response.StatusCode.ToString());
            return null;
        }

        return await _ConstructUserTokenAsync(response);
    }

    public async Task<int> GetUserIdAsync(string accessToken)
    {
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var response = await _httpClient.GetAsync("api/v2/me");
        response.EnsureSuccessStatusCode();
        Console.WriteLine(await response.Content.ReadAsStringAsync());
        var userDataResponse = await response.Content.ReadFromJsonAsync<UserDataResponse>();
        return userDataResponse?.Id ?? throw this.ExceptionSince("User data response is null");
    }

    private async Task<UserToken> _ConstructUserTokenAsync(HttpResponseMessage response)
    {
        var tokenResponse = await response.Content.ReadFromJsonAsync<TokenResponse>();

        if (tokenResponse == null)
        {
            throw this.ExceptionSince("Token response is null");
        }

        return new UserToken
        {
            AccessToken = tokenResponse.AccessToken,
            RefreshToken = tokenResponse.RefreshToken,
            ExpiresAt = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn)
        };
    }
}
