namespace Trinity.PaymentPlatform.Infastructure.ACL.Mpesa.Models.Mpesa
{
    public class MpesaPayoutRequest
    {
        public decimal Amount { get; set; }
        public int UserId { get; set; }
        public string AccountNumber { get; set; } 
    }
}
