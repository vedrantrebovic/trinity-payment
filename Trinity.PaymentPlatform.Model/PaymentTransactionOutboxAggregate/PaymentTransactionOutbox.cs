using Trinity.PaymentPlatform.Model.Enum;
using Trinity.PaymentPlatform.Model.PaymentTransactionAggregate;
using Trinity.PaymentPlatform.Model.SeedWork;
using Trinity.PaymentPlatform.Model.SharedKernel;

namespace Trinity.PaymentPlatform.Model.PaymentTransactionOutboxAggregate;

public class PaymentTransactionOutbox:Entity<long>,IAggregateRoot
{
    public virtual string TransactionReference { get; protected set; }
    public virtual string? ProviderTransactionId { get; protected set; }
    public virtual long TransactionId { get; protected set; }
    public virtual Money Amount { get; protected set; }
    public virtual string CallbackUrl { get; protected set; }
    public virtual bool Sent { get; protected set; }
    public virtual bool Success { get; protected set; }
    public virtual string? Error { get; protected set; }
    public virtual int ResendCount { get; protected set; }
    public virtual DateTime? BackOffUntil { get; protected set; }

    public virtual TransactionType Type { get; protected set; }

    protected PaymentTransactionOutbox() { }

    private PaymentTransactionOutbox(string transactionReference, string? providerTransactionId, long transactionId, Money amount, string url, bool success, string? error, TransactionType type)
    {
        TransactionReference = transactionReference;
        ProviderTransactionId = providerTransactionId;
        TransactionId = transactionId;
        Amount = amount;
        CallbackUrl = url;
        Success = success;
        Error = error;
        Type = type;
    }

    #region Factory

    public static PaymentTransactionOutbox CreatePayin(string transactionReference, string? providerTransactionId,
        long transactionId, Money amount, string url, bool success, string? error)
    {
        return new PaymentTransactionOutbox(transactionReference, providerTransactionId, transactionId, amount, url,
            success, error, TransactionType.PAYIN);
    }

    public static PaymentTransactionOutbox CreatePayout(string transactionReference, string? providerTransactionId,
        long transactionId, Money amount, string url, bool success, string? error)
    {
        return new PaymentTransactionOutbox(transactionReference, providerTransactionId, transactionId, amount, url,
            success, error, TransactionType.PAYOUT);
    }

    public static PaymentTransactionOutbox Create(PaymentTransaction transaction, string url)
    {
        return new PaymentTransactionOutbox(transaction.TransactionId, transaction.ProviderTransactionId, transaction.Id, transaction.Amount, url, 
            transaction.Status == PaymentTransactionStatus.Completed, transaction.Error, transaction.Type);
    }

    #endregion

    public virtual void IncreaseResendCount()
    {
        ResendCount++;
        //if failed too many times back off for 5 minutes in order not to slow down other transactions
        if (ResendCount > 10)
        {
            BackOffUntil = DateTime.UtcNow.AddMinutes(5);
        }
    }

    public virtual void MarkAsSent()
    {
        Sent = true;
    }
}