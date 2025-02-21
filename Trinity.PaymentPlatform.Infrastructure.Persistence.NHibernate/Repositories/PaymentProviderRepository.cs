using Trinity.PaymentPlatform.Model.PaymentProviderAggregate;
using Trinity.PaymentPlatform.Model.SeedWork;

namespace Trinity.PaymentPlatform.Infrastructure.Persistence.NHibernate.Repositories;

public class PaymentProviderRepository(IUnitOfWork unitOfWork): IPaymentProviderRepository
{
    private readonly NHUnitOfWork _unitOfWork = (NHUnitOfWork)unitOfWork;
    public async Task<PaymentProvider> GetAsync(int id)
    {
        return await _unitOfWork.Session.GetAsync<PaymentProvider>(id);
    }

    public async Task SaveAsync(PaymentProvider obj)
    {
        await _unitOfWork.Session.SaveAsync(obj);
    }

    public async Task UpdateAsync(PaymentProvider obj)
    {
        await _unitOfWork.Session.UpdateAsync(obj);
    }

    public async Task DeleteAsync(PaymentProvider obj)
    {
        throw new NotSupportedException();
    }
}