namespace Trinity.PaymentPlatform.Infastructure.ACL.Mpesa.Models.Mpesa
{
    public class MpesaPayInRequest 
    {
        public int UserId { get; set; }
        public decimal Amount { get; set; }
        public string AccountNumber { get; set; }
    }
}
