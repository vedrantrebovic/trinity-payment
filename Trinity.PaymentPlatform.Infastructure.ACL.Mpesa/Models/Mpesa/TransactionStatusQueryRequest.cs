using System.Text.Json.Serialization;

namespace Trinity.PaymentPlatform.Infastructure.ACL.Mpesa.Models.Mpesa
{
    public class TransactionStatusQueryRequest
    {
        [JsonPropertyName("Initiator")]
        public string Initiator { get; set; }

        [JsonPropertyName("SecurityCredential")]
        public string SecurityCredential { get; set; }

        [JsonPropertyName("CommandID")]
        public string CommandID { get; set; }

        [JsonPropertyName("TransactionID")]
        public string TransactionID { get; set; }

        [JsonPropertyName("OriginalConversationID")]
        public string OriginalConversationID { get; set; }

        [JsonPropertyName("PartyA")]
        public string PartyA { get; set; }

        [JsonPropertyName("IdentifierType")]
        public string IdentifierType { get; set; }

        [JsonPropertyName("ResultURL")]
        public string ResultURL { get; set; }

        [JsonPropertyName("QueueTimeOutURL")]
        public string QueueTimeOutURL { get; set; }

        [JsonPropertyName("Remarks")]
        public string Remarks { get; set; }

        [JsonPropertyName("Occasion")]
        public string Occasion { get; set; }
    }

}
