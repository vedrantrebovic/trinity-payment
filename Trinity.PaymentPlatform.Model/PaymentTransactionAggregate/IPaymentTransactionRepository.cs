using Trinity.PaymentPlatform.Model.Enum;
using Trinity.PaymentPlatform.Model.SeedWork;

namespace Trinity.PaymentPlatform.Model.PaymentTransactionAggregate;

public interface IPaymentTransactionRepository:IRepository<PaymentTransaction, long>
{
    Task<PaymentTransaction> GetByTransactionIdAsync(string transactionId);
    Task<MpesaPaymentTransaction> GetTransactionByMerchantIdAsync(string merchantRequestId, string checkoutRequestId);
    Task<IList<PaymentTransaction>> GetTransactions(int providerId, TransactionType type, PaymentTransactionStatus status, int checkDelay, int limit);
    Task<IList<MpesaPaymentTransaction>> GetTransactions(int providerId, TransactionType type, MpesaPaymentTransactionStatus status, int checkDelay, int limit);
}