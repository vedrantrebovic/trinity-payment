﻿using NHibernate.Criterion;
using Trinity.PaymentPlatform.Model.Enum;
using Trinity.PaymentPlatform.Model.PaymentTransactionAggregate;
using Trinity.PaymentPlatform.Model.SeedWork;
using Trinity.PaymentPlatform.Model.Util;

namespace Trinity.PaymentPlatform.Infrastructure.Persistence.NHibernate.Repositories;

public class PaymentTransactionRepository(IUnitOfWork unitOfWork) : IPaymentTransactionRepository
{
    private readonly NHUnitOfWork _unitOfWork = (NHUnitOfWork)unitOfWork;
    public async Task<PaymentTransaction> GetAsync(long id)
    {
        return await _unitOfWork.Session.GetAsync<PaymentTransaction>(id);
    }

    public async Task SaveAsync(PaymentTransaction obj)
    {
        await _unitOfWork.Session.SaveAsync(obj);
    }

    public async Task UpdateAsync(PaymentTransaction obj)
    {
        await _unitOfWork.Session.UpdateAsync(obj);
    }

    public async Task DeleteAsync(PaymentTransaction obj)
    {
        throw new NotSupportedException();
    }

    public async Task<PaymentTransaction> GetByTransactionIdAsync(string transactionId)
    {
        return await _unitOfWork.Session.QueryOver<PaymentTransaction>().Where(p => p.TransactionId == transactionId)
            .SingleOrDefaultAsync();
    }

    public async Task<MpesaPaymentTransaction> GetTransactionByMerchantIdAsync(string merchantRequestId, string checkoutRequestId)
    {
        return await _unitOfWork.Session.QueryOver<MpesaPaymentTransaction>()
            .Where(p => p.TransactionId == merchantRequestId && p.ProviderTransactionId == checkoutRequestId)
            .SingleOrDefaultAsync();
    }

    public async Task<IList<PaymentTransaction>> GetTransactions(int providerId, TransactionType type, PaymentTransactionStatus status, int checkDelay, int limit)
    {
        var criteria = _unitOfWork.Session.CreateCriteria<PaymentTransaction>()
            .Add(Restrictions.Eq(nameof(PaymentTransaction.ProviderId), providerId))
            .Add(Restrictions.Eq(nameof(PaymentTransaction.Type), type))
            .Add(Restrictions.Eq(nameof(PaymentTransaction.Status), status))
            .Add(Restrictions.Eq(nameof(PaymentTransaction.ModifiedAt),
                DateTime.UtcNow.AddMinutes(-1 * checkDelay).ToUnixTimestampMilliseconds()));
        criteria.AddOrder(Order.Desc(nameof(PaymentTransaction.CreatedAt)));
        criteria.SetMaxResults(limit);

        return await criteria.ListAsync<PaymentTransaction>();
    }

    public async Task<IList<MpesaPaymentTransaction>> GetTransactions(int providerId, TransactionType type, MpesaPaymentTransactionStatus status, int checkDelay,
        int limit)
    {
        var criteria = _unitOfWork.Session.CreateCriteria<MpesaPaymentTransaction>()
            .Add(Restrictions.Eq(nameof(MpesaPaymentTransaction.ProviderId), providerId))
            .Add(Restrictions.Eq(nameof(MpesaPaymentTransaction.Type), type))
            .Add(Restrictions.Eq(nameof(MpesaPaymentTransaction.MpesaStatus), status))
            .Add(Restrictions.Eq(nameof(MpesaPaymentTransaction.ModifiedAt),
                DateTime.UtcNow.AddMinutes(-1 * checkDelay).ToUnixTimestampMilliseconds()));
        criteria.AddOrder(Order.Desc(nameof(PaymentTransaction.CreatedAt)));
        criteria.SetMaxResults(limit);

        return await criteria.ListAsync<MpesaPaymentTransaction>();
    }
}