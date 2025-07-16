using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Trinity.PaymentPlatform.Infrastructure.ACL.Airtel.Model
{
    public class AirtelCallbackRequest
    {
        [JsonPropertyName("transaction")]
        public AirtelCallbackTransaction Transaction { get; set; }

        [JsonPropertyName("hash")]
        public string Hash { get; set; }
    }

    public class AirtelCallbackTransaction
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }

        [JsonPropertyName("status_code")]
        public string StatusCode { get; set; }

        [JsonPropertyName("airtel_money_id")]
        public string AirtelMoneyId { get; set; }
    }
}
