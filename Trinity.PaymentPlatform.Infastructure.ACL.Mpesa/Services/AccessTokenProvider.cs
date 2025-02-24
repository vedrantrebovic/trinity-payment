using FluentResults;
using Microsoft.Extensions.Caching.Memory;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Trinity.PaymentPlatform.Infrastructure.ACL.Mpesa.Contracts;
using Trinity.PaymentPlatform.Infrastructure.ACL.Mpesa.Models.Mpesa;

namespace Trinity.PaymentPlatform.Infrastructure.ACL.Mpesa.Services;

public class AccessTokenProvider(ILogger<AccessTokenProvider> logger, IMemoryCache memoryCache, 
    IHttpClientFactory httpClientFactory, IOptions<MpesaConfig> mpesaConfig): IAccessTokenProvider
{
    private readonly string _cacheKey = $"Mpesa:Token:{PaymentProviderId}";
    private const int PaymentProviderId = 1;
    private string _apiKey = mpesaConfig.Value.CustomerKey;
    private string _apiSecret = mpesaConfig.Value.CustomerSecret;

    private readonly HttpClient _httpClient = httpClientFactory.CreateClient("MpesaUnauthorized");


    public string? GetToken()
    {
        if (memoryCache.TryGetValue<string>(_cacheKey, out var value))
            return value;

        return null;
    }

    public async Task<Result> ObtainToken()
    {
        var encodedString = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_apiKey}:{_apiSecret}"));
        using var request = new HttpRequestMessage(
            HttpMethod.Get, "oauth/v1/generate?grant_type=client_credentials");
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", encodedString);

        var response = await _httpClient.SendAsync(request);
        if (response.IsSuccessStatusCode)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            var tokenResponse = JsonSerializer.Deserialize<OAuthResponse>(responseContent);

            string token = tokenResponse.AccessToken;

            int expiresInSeconds = Convert.ToInt16(tokenResponse.ExpiresIn);
            var cacheEntryOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(expiresInSeconds - 60)
            };

            memoryCache.Set(_cacheKey, token, cacheEntryOptions);
            return Result.Ok();
        }

        var body = response.Content.ReadAsStringAsync();
        logger.LogCritical(
            $"Cannot obtain access token from MPESA. API response code: {response.StatusCode}. Body response: {body}");
        return Result.Fail("err.obtain_token_failed");
    }
}