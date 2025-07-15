using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Trinity.PaymentPlatform.Infrastructure.ACL.Airtel.Model
{
    public record AirtelPayInRequest(string UserId, decimal Amount, string CurrencyCode, string AccountNumber, string TransactionReference, string Country);

    //public class AirtelPayInRequest
    //{

    //    [JsonPropertyName("reference")]
    //    public string Reference { get; set; }

    //    [JsonPropertyName("subscriber")]
    //    public AirtelSubscriber Subscriber { get; set; }

    //    [JsonPropertyName("transaction")]
    //    public AirtelTransaction Transaction { get; set; }

    //}
    //public class AirtelSubscriber
    //{
    //    [JsonPropertyName("country")]
    //    public string Country { get; set; }

    //    [JsonPropertyName("currency")]
    //    public string Currency { get; set; }

    //    [JsonPropertyName("msisdn")]
    //    public string Msisdn { get; set; }
    //}

    //public class AirtelTransaction
    //{
    //    [JsonPropertyName("amount")]
    //    public decimal Amount { get; set; }

    //    [JsonPropertyName("country")]
    //    public string Country { get; set; }

    //    [JsonPropertyName("currency")]
    //    public string Currency { get; set; }

    //    [JsonPropertyName("id")]
    //    public string Id { get; set; }
    //}

}
