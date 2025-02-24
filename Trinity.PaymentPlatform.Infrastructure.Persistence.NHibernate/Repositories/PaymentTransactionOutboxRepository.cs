using NHibernate.Linq;
using Trinity.PaymentPlatform.Model.PaymentTransactionOutboxAggregate;
using Trinity.PaymentPlatform.Model.SeedWork;

namespace Trinity.PaymentPlatform.Infrastructure.Persistence.NHibernate.Repositories;

public class PaymentTransactionOutboxRepository(IUnitOfWork unitOfWork) : IPaymentTransactionOutboxRepository
{
    private readonly NHUnitOfWork _unitOfWork = (NHUnitOfWork)unitOfWork;
    public async Task<PaymentTransactionOutbox> GetAsync(long id)
    {
        return await _unitOfWork.Session.GetAsync<PaymentTransactionOutbox>(id);
    }

    public async Task SaveAsync(PaymentTransactionOutbox obj)
    {
        await _unitOfWork.Session.SaveAsync(obj);
    }

    public async Task UpdateAsync(PaymentTransactionOutbox obj)
    {
        await _unitOfWork.Session.UpdateAsync(obj);
    }

    public async Task DeleteAsync(PaymentTransactionOutbox obj)
    {
        throw new NotSupportedException();
    }

    public async Task<IList<PaymentTransactionOutbox>> GetUnsent()
    {
        return await _unitOfWork.Session.Query<PaymentTransactionOutbox>().Where(x => !x.Sent &&
            (x.BackOffUntil == null || x.BackOffUntil.Value < DateTime.UtcNow)).ToListAsync();
    }
}