using System.Text.Json.Serialization;

namespace Trinity.PaymentPlatform.Infrastructure.ACL.Mpesa.Models.Mpesa
{
    public class OAuthResponse
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; }
        [JsonPropertyName("expires_in")]
        public string ExpiresIn { get; set; }

    }
}
