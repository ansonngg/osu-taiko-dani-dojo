using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Application.Interface;
using Application.Model;
using Application.Options;
using Application.Utility;
using Infrastructure.Response;
using Microsoft.Extensions.Options;

namespace Infrastructure.Service;

public class RedisSessionService : IRedisSessionService
{
    private static readonly int SessionExpiryInSecond = (int)TimeSpan.FromDays(14).TotalSeconds;
    private readonly HttpClient _httpClient;

    public RedisSessionService(HttpClient httpClient, IOptions<RedisOptions> options)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri(options.Value.Url);
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", options.Value.Token);
    }

    public async Task SaveSessionAsync(string sessionId, object sessionData)
    {
        var dataJson = JsonSerializer.Serialize(sessionData);
        var bodyParams = new object[] { "SET", sessionId, dataJson, "EX", SessionExpiryInSecond };
        await _httpClient.PostAsJsonAsync("", bodyParams);
    }

    public async Task<UserSession?> GetSessionAsync(string sessionId)
    {
        var bodyParams = new { command = new object[] { "GET", sessionId } };
        var response = await _httpClient.PostAsJsonAsync("", bodyParams);
        response.EnsureSuccessStatusCode();
        var retrieveDataResponse = await response.Content.ReadFromJsonAsync<RetrieveDataResponse>();

        return retrieveDataResponse != null
            ? JsonSerializer.Deserialize<UserSession>(retrieveDataResponse.Result)
            : throw this.ExceptionSince("Retrieve data response is null.");
    }

    public async Task DeleteSessionAsync(string sessionId)
    {
        var bodyParams = new { command = new object[] { "DEL", sessionId } };
        var response = await _httpClient.PostAsJsonAsync("", bodyParams);
        response.EnsureSuccessStatusCode();
    }

    public async Task<bool> ExistsSessionAsync(string sessionId)
    {
        var bodyParams = new object[] { "EXISTS", sessionId };
        var response = await _httpClient.PostAsJsonAsync("", bodyParams);
        Console.WriteLine(await response.Content.ReadAsStringAsync());
        response.EnsureSuccessStatusCode();
        var existKeyResponse = await response.Content.ReadFromJsonAsync<ExistKeyResponse>();

        if (existKeyResponse == null)
        {
            throw this.ExceptionSince("Exist key response is null.");
        }

        return existKeyResponse.Result == 1;
    }
}
