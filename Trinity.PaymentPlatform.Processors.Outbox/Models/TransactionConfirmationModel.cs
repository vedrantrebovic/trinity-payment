namespace Trinity.PaymentPlatform.Processors.Outbox.Models;

public record TransactionConfirmationModel(string TransactionReference, string ProviderTransactionReference, decimal Amount, string CurrencyCode,  bool Success, string? Error);