using Trinity.PaymentPlatform.Model.Contracts;

namespace Trinity.PaymentPlatform.Application.Models;

public record PayoutModel(string UserId, decimal Amount, string CurrencyCode, string TransactionReference): IPayoutTransactionInputParams;

public record MpesaPayoutModel(string UserId,
    decimal Amount,
    string CurrencyCode,
    string TransactionReference,
    string PhoneNumber) : PayoutModel(UserId, Amount, CurrencyCode, TransactionReference);