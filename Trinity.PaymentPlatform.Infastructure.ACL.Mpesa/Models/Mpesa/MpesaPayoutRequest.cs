namespace Trinity.PaymentPlatform.Infrastructure.ACL.Mpesa.Models.Mpesa
{
    public record MpesaPayoutRequest(
        int UserId,
        decimal Amount,
        string CurrencyCode,
        string AccountNumber,
        string TransactionReference);
}
