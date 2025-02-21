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

    protected MpesaPaymentTransaction() { }

    protected MpesaPaymentTransaction(int providerId, string userId, Money amount, TransactionType type,
        string accountNumber,
        string transactionId, string providerTransactionId, string merchantRequestId, string checkoutRequestId)
    :base(providerId, userId, amount, type, accountNumber, transactionId, providerTransactionId, merchantRequestId, checkoutRequestId)
    {
        MpesaStatus = MpesaPaymentTransactionStatus.PENDING;
        AccountNumber = accountNumber;
    }

    public static MpesaPaymentTransaction CreatePayIn(int providerId, string userId, Money amount,
        string accountNumber,
        string transactionId, string providerTransactionId, string merchantRequestId, string checkoutRequestId)
    {
        return new MpesaPaymentTransaction(providerId, userId, amount, TransactionType.PAYIN, accountNumber, transactionId,
            providerTransactionId, merchantRequestId, checkoutRequestId);
    }

    public static MpesaPaymentTransaction CreatePayOut(int providerId, string userId, Money amount,
        string accountNumber,
        string transactionId, string providerTransactionId, string merchantRequestId, string checkoutRequestId)
    {
        return new MpesaPaymentTransaction(providerId, userId, amount, TransactionType.PAYOUT, accountNumber, transactionId,
            providerTransactionId, merchantRequestId, checkoutRequestId);
    }

    public virtual Result SetInProgress()
    {
        //todo: check if status change is valid
        MpesaStatus = MpesaPaymentTransactionStatus.IN_PROGRESS;
        Status = PaymentTransactionStatus.InProgress;
        return Result.Ok();
    }

    public virtual Result SetFailed(string error, string providerTransactionId)
    {
        //todo: check if status change is valid
        MpesaStatus = MpesaPaymentTransactionStatus.FAILED;
        Status = PaymentTransactionStatus.Failed;
        ProviderTransactionId = providerTransactionId;
        Error = error;
        base.SetFailed(error);
        return Result.Ok();
    }

    public override Result SetCompleted()
    {
        //todo: check if status change is valid
        MpesaStatus = MpesaPaymentTransactionStatus.COMPLETED;
        Status = PaymentTransactionStatus.Completed;
        base.SetCompleted();
        return Result.Ok();
    }

    public virtual Result SetCanceled(string? error)
    {
        MpesaStatus = MpesaPaymentTransactionStatus.CANCELED;
        Status = PaymentTransactionStatus.Cancelled;
        base.SetCancelled(error);
        return Result.Ok();
    }

    public virtual void SetRequestId(string merchantRequestId, string checkoutRequestId)
    {
        MerchantRequestId = merchantRequestId;
        CheckoutRequestId = checkoutRequestId;
    }
}