using Trinity.PaymentPlatform.Model.Enum;
using Trinity.PaymentPlatform.Model.SeedWork;

namespace Trinity.PaymentPlatform.Model.PaymentTransactionAggregate;

public interface IPaymentTransactionRepository:IRepository<PaymentTransaction, long>
{
    Task<PaymentTransaction?> GetByTransactionIdAsync(string transactionId);
    Task<AirtelPaymentTransaction?> GetAirtelTransactionIdAsync(string transactionId);
  //  Task<AirtelPaymentTransaction?> GetAirtelMoneyIdAsync(string airtelMoneyId);
    Task<MpesaPaymentTransaction?> GetTransactionByMerchantIdAsync(string merchantRequestId, string checkoutRequestId);
    Task<IList<PaymentTransaction>> GetTransactions(int providerId, TransactionType type, PaymentTransactionStatus status, int checkDelay, int limit);
    Task<IList<MpesaPaymentTransaction>> GetTransactions(int providerId, TransactionType type, MpesaPaymentTransactionStatus status, int checkDelay, int limit);
    Task<IList<AirtelPaymentTransaction>> GetAirtelTransactions(int providerId, TransactionType type, AirtelPaymentTransactionStatus status, int checkDelay, int limit);
    Task<bool> FindIfExistsAsync(string transactionReference, TransactionType type);
    Task<IList<PaymentTransaction>> GetFinalizedForOutbox();
}