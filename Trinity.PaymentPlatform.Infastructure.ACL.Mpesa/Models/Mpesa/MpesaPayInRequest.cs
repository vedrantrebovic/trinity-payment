namespace Trinity.PaymentPlatform.Infastructure.ACL.Mpesa.Models.Mpesa
{
    public record MpesaPayInRequest(int UserId, decimal Amount, string CurrencyCode, string AccountNumber, string TransactionReference);
}
