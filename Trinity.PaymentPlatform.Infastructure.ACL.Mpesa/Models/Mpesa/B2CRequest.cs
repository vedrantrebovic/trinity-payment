using System.Text.Json.Serialization;

namespace Trinity.PaymentPlatform.Infrastructure.ACL.Mpesa.Models.Mpesa
{
    public class B2CRequest
    {
        [JsonPropertyName("OriginatorConversationID")]
        public string OriginatorConversationID { get; set; }

        [JsonPropertyName("InitiatorName")]
        public string InitiatorName { get; set; }

        [JsonPropertyName("SecurityCredential")]
        public string SecurityCredential { get; set; }

        [JsonPropertyName("CommandID")]
        public string CommandID { get; set; }

        [JsonPropertyName("Amount")]
        public decimal Amount { get; set; }

        [JsonPropertyName("PartyA")]
        public string PartyA { get; set; }

        [JsonPropertyName("PartyB")]
        public string PartyB { get; set; }

        [JsonPropertyName("Remarks")]
        public string Remarks { get; set; }

        [JsonPropertyName("QueueTimeOutURL")]
        public string QueueTimeOutURL { get; set; }

        [JsonPropertyName("ResultURL")]
        public string ResultURL { get; set; }
    }
}
