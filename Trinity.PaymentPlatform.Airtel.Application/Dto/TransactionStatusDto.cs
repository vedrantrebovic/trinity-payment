namespace Trinity.PaymentPlatform.Airtel.Application.Dto;

public record TransactionStatusDto(long Id, string TransactionReference, int ProviderId, string UserId, int Type, string TypeText, string? ProviderTransactionReference, 
    string? Error, int Status, string StatusText);
