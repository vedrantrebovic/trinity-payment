using FluentResults;
using Trinity.PaymentPlatform.Model.Enum;
using Trinity.PaymentPlatform.Model.SharedKernel;

namespace Trinity.PaymentPlatform.Model.PaymentTransactionAggregate;

public class MpesaPaymentTransaction:PaymentTransaction
{
    public virtual MpesaPaymentTransactionStatus MpesaStatus { get; protected set; }
    public virtual string AccountNumber { get; protected set; }
    public virtual string MerchantRequestId { get; protected set; }
    public virtual string CheckoutRequestId { get; protected set; }
    public virtual string ProviderTimestamp { get; protected set; }

    protected MpesaPaymentTransaction() { }

    protected MpesaPaymentTransaction(int providerId, string userId, Money amount, TransactionType type,
        string accountNumber,
        string transactionId)
    :base(providerId, userId, amount, type, accountNumber, transactionId)
    {
        MpesaStatus = MpesaPaymentTransactionStatus.PENDING;
        AccountNumber = accountNumber;
    }

    public static MpesaPaymentTransaction CreatePayIn(int providerId, string userId, Money amount,
        string accountNumber,
        string transactionId)
    {
        return new MpesaPaymentTransaction(providerId, userId, amount, TransactionType.PAYIN, accountNumber, transactionId);
    }

    public static MpesaPaymentTransaction CreatePayOut(int providerId, string userId, Money amount,
        string accountNumber,
        string transactionId)
    {
        return new MpesaPaymentTransaction(providerId, userId, amount, TransactionType.PAYOUT, accountNumber, transactionId);
    }

    public virtual Result SetInProgress()
    {
        //todo: check if status change is valid
        MpesaStatus = MpesaPaymentTransactionStatus.IN_PROGRESS;
        Status = PaymentTransactionStatus.InProgress;
        return Result.Ok();
    }

    public new virtual Result SetFailed(string? error)
    {
        //todo: check if status change is valid
        MpesaStatus = MpesaPaymentTransactionStatus.FAILED;
        Status = PaymentTransactionStatus.Failed;
        Error = error;
        base.SetFailed(error);
        return Result.Ok();
    }

    public new virtual Result SetCompleted()
    {
        //todo: check if status change is valid
        MpesaStatus = MpesaPaymentTransactionStatus.COMPLETED;
        Status = PaymentTransactionStatus.Completed;
        base.SetCompleted();
        return Result.Ok();
    }

    public new virtual Result SetCanceled(string? error)
    {
        MpesaStatus = MpesaPaymentTransactionStatus.CANCELED;
        Status = PaymentTransactionStatus.Cancelled;
        base.SetCancelled(error);
        return Result.Ok();
    }

    public virtual void SetRequestId(string? merchantRequestId, string? checkoutRequestId)
    {
        MerchantRequestId = merchantRequestId;
        CheckoutRequestId = checkoutRequestId;
    }
    
    public virtual void SetProviderTimestamp(string providerTimestamp)
    {
        ProviderTimestamp = providerTimestamp;
    }
}