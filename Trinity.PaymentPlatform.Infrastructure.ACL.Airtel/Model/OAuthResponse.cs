using System.Text.Json.Serialization;

namespace Trinity.PaymentPlatform.Infrastructure.ACL.Airtel.Model
{
    public class OAuthResponse
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; }
        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }

    }
}
