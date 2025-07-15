using Trinity.PaymentPlatform.Model.Contracts;

namespace Trinity.PaymentPlatform.Application.Models;

public record PayInModel(string UserId, decimal Amount, string CurrencyCode, string TransactionReference) : IPayInTransactionInputParams;

public record MpesaPayInModel(string UserId, decimal Amount, string CurrencyCode, string TransactionReference, string PhoneNumber) :
    PayInModel(UserId, Amount, CurrencyCode, TransactionReference);

public record AirtelPayInModel(
    string UserId,
    decimal Amount,
    string CurrencyCode,
    string Country,
    string TransactionReference,
    string PhoneNumber
) : PayInModel(UserId, Amount, CurrencyCode, TransactionReference);