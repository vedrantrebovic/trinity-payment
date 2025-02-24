using FluentResults;
using Trinity.PaymentPlatform.Model.Enum;
using Trinity.PaymentPlatform.Model.SharedKernel;

namespace Trinity.PaymentPlatform.Model.PaymentTransactionAggregate;

public class MpesaPaymentTransaction:PaymentTransaction
{
    public virtual MpesaPaymentTransactionStatus MpesaStatus { get; protected set; }
    public virtual string AccountNumber { get; protected set; }
    public virtual string MerchantRequestId { get; protected set; }
    public virtual string? CheckoutRequestId { get; protected set; }
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
        return base.SetInProgress();
    }

    public virtual Result SetInProgress(string? checkoutRequestId)
    {
        //todo: check if status change is valid
        MpesaStatus = MpesaPaymentTransactionStatus.IN_PROGRESS;
        CheckoutRequestId = checkoutRequestId;
        return base.SetInProgress();
    }

    public new virtual Result SetFailed(string? error)
    {
        //todo: check if status change is valid
        MpesaStatus = MpesaPaymentTransactionStatus.FAILED;
        Status = PaymentTransactionStatus.Failed;
        Error = error;
        return base.SetFailed(error);
    }

    public virtual Result Complete()
    {
        //todo: check if status change is valid
        MpesaStatus = MpesaPaymentTransactionStatus.COMPLETED;
        Status = PaymentTransactionStatus.Completed;
        return base.SetCompleted();
    }

    public new virtual Result SetUnconfirmed(string? error)
    {
        //todo: check if status change is valid
        MpesaStatus = MpesaPaymentTransactionStatus.UNCONFIRMED;
        Error = error;
        return base.SetAwaitingConfirmation();
    }

    public new virtual Result SetCanceled(string? error)
    {
        MpesaStatus = MpesaPaymentTransactionStatus.CANCELED;
        Status = PaymentTransactionStatus.Cancelled;
        return base.SetCancelled(error);
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