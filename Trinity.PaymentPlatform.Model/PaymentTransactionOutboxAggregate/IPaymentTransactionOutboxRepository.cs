using Trinity.PaymentPlatform.Model.SeedWork;

namespace Trinity.PaymentPlatform.Model.PaymentTransactionOutboxAggregate;

public interface IPaymentTransactionOutboxRepository:IRepository<PaymentTransactionOutbox, long>
{
    Task<IList<PaymentTransactionOutbox>> GetUnsent();
}