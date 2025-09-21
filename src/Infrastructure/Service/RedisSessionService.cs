﻿using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Options;
using OsuTaikoDaniDojo.Application.Interface;
using OsuTaikoDaniDojo.Application.Model;
using OsuTaikoDaniDojo.Application.Options;
using OsuTaikoDaniDojo.Application.Utility;
using OsuTaikoDaniDojo.Infrastructure.Response;

namespace OsuTaikoDaniDojo.Infrastructure.Service;

public class RedisSessionService : ISessionService
{
    private readonly HttpClient _httpClient;
    private readonly int _redisSessionExpiryInSecond;

    public RedisSessionService(
        HttpClient httpClient,
        IOptions<SessionOptions> sessionOptions,
        IOptions<RedisOptions> redisOptions)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri(redisOptions.Value.Url);
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            redisOptions.Value.Token);

        _redisSessionExpiryInSecond = (int)TimeSpan.FromDays(sessionOptions.Value.RedisExpiryInDay).TotalSeconds;
    }

    public async Task SaveSessionAsync(string sessionId, object sessionData)
    {
        var dataJson = JsonSerializer.Serialize(sessionData);
        var bodyParams = new object[] { "SET", sessionId, dataJson, "EX", _redisSessionExpiryInSecond };
        await _httpClient.PostAsJsonAsync("", bodyParams);
    }

    public async Task<UserSession?> GetSessionAsync(string sessionId)
    {
        var bodyParams = new object[] { "GET", sessionId };
        var response = await _httpClient.PostAsJsonAsync("", bodyParams);
        response.EnsureSuccessStatusCode();
        var retrieveDataResponse = await response.Content.ReadFromJsonAsync<RetrieveDataResponse>();

        return retrieveDataResponse != null
            ? JsonSerializer.Deserialize<UserSession>(retrieveDataResponse.Result)
            : throw this.ExceptionSince("Retrieve data response is null.");
    }

    public async Task DeleteSessionAsync(string sessionId)
    {
        var bodyParams = new object[] { "DEL", sessionId };
        var response = await _httpClient.PostAsJsonAsync("", bodyParams);
        response.EnsureSuccessStatusCode();
    }

    public async Task<bool> ExistsSessionAsync(string sessionId)
    {
        var bodyParams = new object[] { "EXISTS", sessionId };
        var response = await _httpClient.PostAsJsonAsync("", bodyParams);
        response.EnsureSuccessStatusCode();
        var existKeyResponse = await response.Content.ReadFromJsonAsync<ExistKeyResponse>();

        if (existKeyResponse == null)
        {
            throw this.ExceptionSince("Exist key response is null.");
        }

        return existKeyResponse.Result == 1;
    }
}
