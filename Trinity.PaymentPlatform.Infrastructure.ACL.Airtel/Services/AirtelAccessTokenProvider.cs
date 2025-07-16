using FluentResults;
using Microsoft.Extensions.Caching.Memory;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Trinity.PaymentPlatform.Infrastructure.ACL.Airtel.Contracts;
using Trinity.PaymentPlatform.Infrastructure.ACL.Airtel.Model;
using Trinity.PaymentPlatform.Infrastructure.ACL.Airtel;

namespace Trinity.PaymentPlatform.Infrastructure.ACL.Airtel.Services;
public class AirtelAccessTokenProvider(
    ILogger<AirtelAccessTokenProvider> logger,
    IMemoryCache memoryCache,
    IHttpClientFactory httpClientFactory,
    IOptions<AirtelConfig> airtelConfig) : IAccessTokenProvider
{
    private readonly string _cacheKey = $"Airtel:Token:{PaymentProviderId}";
    private const int PaymentProviderId = 2;

    private readonly string _clientId = airtelConfig.Value.ClientId;
    private readonly string _clientSecret = airtelConfig.Value.CustomerSecret;
    private readonly HttpClient _httpClient = httpClientFactory.CreateClient("AirtelUnauthorized");

    public string? GetToken()
    {
        if (memoryCache.TryGetValue<string>(_cacheKey, out var value))
            return value;

        return null;
    }

    public async Task<Result> ObtainToken()
    {
        var requestPayload = new
        {
            client_id = _clientId,
            client_secret = _clientSecret,
            grant_type = "client_credentials"
        };

        var content = new StringContent(JsonSerializer.Serialize(requestPayload), Encoding.UTF8, "application/json");

        using var request = new HttpRequestMessage(HttpMethod.Post, "/auth/oauth2/token")
        {
            Content = content
        };

        var response = await _httpClient.SendAsync(request);
        var responseBody = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
        {
            var tokenResponse = JsonSerializer.Deserialize<OAuthResponse>(responseBody);

            string? token = tokenResponse?.AccessToken;
            if (!string.IsNullOrEmpty(token))
            {
                int expiresInSeconds = tokenResponse.ExpiresIn;

                var cacheEntryOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(expiresInSeconds - 60)
                };

                memoryCache.Set(_cacheKey, token, cacheEntryOptions);
                return Result.Ok();
            }
        }

        logger.LogCritical(
            $"Cannot obtain Airtel access token. StatusCode: {response.StatusCode}. Body: {responseBody}");

        return Result.Fail("err.obtain_token_failed");
    }

}