using FluentResults;
using Trinity.PaymentPlatform.Model.Enum;
using Trinity.PaymentPlatform.Model.SeedWork;
using Trinity.PaymentPlatform.Model.SharedKernel;

namespace Trinity.PaymentPlatform.Model.PaymentTransactionAggregate;

public class PaymentTransaction : Entity<long>, IAggregateRoot
{
    public virtual int ProviderId { get; protected set; }
    public virtual string UserId { get; protected set; }
    public virtual Money Amount { get; protected set; }
    public virtual TransactionType Type { get; protected set; }
    public virtual string TransactionId { get; protected set; }
    public virtual string ProviderTransactionId { get; protected set; }
    public virtual string? Error { get; protected set; }
    public virtual DateTime? StatusFinalizationTime { get; protected set; }
    public virtual PaymentTransactionStatus Status { get; protected set; }

    protected PaymentTransaction()
    {
    }

    protected PaymentTransaction(int providerId, string userId, Money amount, TransactionType type,
        string accountNumber,
        string transactionId, string providerTransactionId, string merchantRequestId, string checkoutRequestId)
    {
        ProviderId = providerId;
        UserId = userId;
        Type = type;
        TransactionId = transactionId;
        ProviderTransactionId = providerTransactionId;
        Amount = amount;
    }

    protected virtual Result SetInProgress()
    {
        //todo: add state transition validation
        Status = PaymentTransactionStatus.InProgress;
        return Result.Ok();
    }

    protected virtual Result SetAwaitingConfirmation()
    {
        //todo: add state transition validation
        Status = PaymentTransactionStatus.AwaitingConfirmation;
        return Result.Ok();
    }

    public virtual Result SetCompleted()
    {
        //todo: add state transition validation
        Status = PaymentTransactionStatus.Completed;
        StatusFinalizationTime = DateTime.UtcNow;
        return Result.Ok();
    }

    protected virtual Result SetRejected(string? error)
    {
        //todo: add state transition validation
        Status = PaymentTransactionStatus.Rejected;
        Error = error;
        StatusFinalizationTime = DateTime.UtcNow;
        return Result.Ok();
    }

    protected virtual Result SetCancelled(string? error)
    {
        //todo: add state transition validation
        Status = PaymentTransactionStatus.Cancelled;
        Error = error;
        StatusFinalizationTime = DateTime.UtcNow;
        return Result.Ok();
    }

    protected virtual Result SetFailed(string error)
    {
        //todo: add state transition validation
        Status = PaymentTransactionStatus.Failed;
        Error = error;
        StatusFinalizationTime = DateTime.UtcNow;
        return Result.Ok();
    }
}

