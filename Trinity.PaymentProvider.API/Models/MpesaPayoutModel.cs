
public record MpesaPayoutModel(int UserId,
    decimal Amount,
    string CurrencyCode,
    string AccountNumber,
    string TransactionReference);