namespace Trinity.PaymentPlatform.Infrastructure.ACL.Mpesa.Models.Mpesa
{
    public record MpesaPayoutRequest(
        string UserId,
        decimal Amount,
        string CurrencyCode,
        string AccountNumber,
        string TransactionReference);
}
