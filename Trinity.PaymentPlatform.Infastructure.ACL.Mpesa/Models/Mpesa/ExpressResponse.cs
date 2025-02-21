namespace Trinity.PaymentPlatform.Infastructure.ACL.Mpesa.Models.Mpesa
{
    public class ExpressResponse
    {
        public string MerchantRequestID { get; set; }
        public string CheckoutRequestID { get; set; }
        public string ResultCode { get; set; }
        public string ResultDesc { get; set; }
        public string ResponseCode { get; set; }
        public string CallbackMetadata { get; set; }
    }


}
