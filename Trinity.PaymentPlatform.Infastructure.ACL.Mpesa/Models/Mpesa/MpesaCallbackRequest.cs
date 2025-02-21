using System.Text.Json;
using System.Text.Json.Serialization;

namespace Trinity.PaymentPlatform.Infastructure.ACL.Mpesa.Models.Mpesa
{
    public class MpesaCallbackRequest
    {
        [JsonPropertyName("Body")]
        public Body Body { get; set; }
    }

    public class Body
    {
        [JsonPropertyName("stkCallback")]
        public StkCallback StkCallback { get; set; }
    }

    public class StkCallback
    {
        [JsonPropertyName("MerchantRequestID")]
        public string MerchantRequestID { get; set; }

        [JsonPropertyName("CheckoutRequestID")]
        public string CheckoutRequestID { get; set; }

        [JsonPropertyName("ResultCode")]
        public int ResultCode { get; set; }

        [JsonPropertyName("ResultDesc")]
        public string ResultDesc { get; set; }

        [JsonPropertyName("CallbackMetadata")]
        public CallbackMetadata? CallbackMetadata { get; set; }
    }

    public class CallbackMetadata
    {
        [JsonPropertyName("Item")]
        public List<MpesaCallbackItem> Item { get; set; }
    }

    
    public class MpesaCallbackItem : IParameter
    {
        [JsonPropertyName("Name")]
        public string Name { get; set; }

        [JsonPropertyName("Value")]
        public JsonElement Value { get; set; }

        public string Key => Name;
        public object? RawValue => Value;
    }

}