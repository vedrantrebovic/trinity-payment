namespace Trinity.PaymentPlatform.Infrastructure.ACL.Mpesa.Models.Mpesa
{
    public record MpesaPayInRequest(string UserId, decimal Amount, string CurrencyCode, string AccountNumber, string TransactionReference);
}
