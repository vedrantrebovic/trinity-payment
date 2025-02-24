namespace Trinity.PaymentProvider.API.Models;

public record MPesaPayinModel(string UserId, decimal Amount, string CurrencyCode, string AccountNumber, string TransactionReference);