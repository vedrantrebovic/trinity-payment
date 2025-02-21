using System.Text.Json;
using System.Text.Json.Serialization;

namespace Trinity.PaymentPlatform.Infastructure.ACL.Mpesa.Models.Mpesa
{
    public class B2CResultRequestPayout
    {
        [JsonPropertyName("Result")]
        public ResultData Result { get; set; }
    }

    public class ResultData
    {
        [JsonPropertyName("ResultType")]
        public int ResultType { get; set; }

        [JsonPropertyName("ResultCode")]
        public int ResultCode { get; set; }

        [JsonPropertyName("ResultDesc")]
        public string ResultDesc { get; set; }

        [JsonPropertyName("OriginatorConversationID")]
        public string OriginatorConversationID { get; set; }

        [JsonPropertyName("ConversationID")]
        public string ConversationID { get; set; }

        [JsonPropertyName("TransactionID")]
        public string TransactionID { get; set; }

        [JsonPropertyName("ResultParameters")]
        public ResultParameters? ResultParameters { get; set; }

        [JsonPropertyName("ReferenceData")]
        public ReferenceData ReferenceData { get; set; }
    }

    public class ResultParameters
    {
        [JsonPropertyName("ResultParameter")]
        public List<ResultParameter> ResultParameter { get; set; }
    }

    public class ResultParameter : IParameter
    {
        [JsonPropertyName("Key")]
        public string Key { get; set; }

        [JsonPropertyName("Value")]
        public JsonElement Value { get; set; }

        public object? RawValue => Value;
    }

    public class ReferenceData
    {
        [JsonPropertyName("ReferenceItem")]
        public ReferenceItem ReferenceItem { get; set; }
    }

    public class ReferenceItem
    {
        [JsonPropertyName("Key")]
        public string Key { get; set; }

        [JsonPropertyName("Value")]
        public string? Value { get; set; }
    }

}
