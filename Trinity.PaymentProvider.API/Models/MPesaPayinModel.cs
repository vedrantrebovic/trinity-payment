namespace Trinity.PaymentProvider.API.Models;

public record MpesaPayinModel(string UserId, decimal Amount, string CurrencyCode, string AccountNumber, string TransactionReference);