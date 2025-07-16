using System.Diagnostics.Metrics;
using FluentResults;
using ISO._4217.Models;
using Trinity.PaymentPlatform.Model.Enum;
using Trinity.PaymentPlatform.Model.SharedKernel;

namespace Trinity.PaymentPlatform.Model.PaymentTransactionAggregate;

public class AirtelPaymentTransaction:PaymentTransaction
{
    public virtual AirtelPaymentTransactionStatus AirtelStatus { get; protected set; }
    public virtual string AccountNumber { get; protected set; }
    public virtual string Country { get; protected set; }
    public virtual string AirtelTransactionId { get; protected set; }


    protected AirtelPaymentTransaction() { }

    protected AirtelPaymentTransaction(int providerId, string userId, Money amount, TransactionType type,
        string accountNumber,
        string transactionId, string country)
    : base(providerId, userId, amount, type, accountNumber, transactionId)
    {
        AirtelStatus = AirtelPaymentTransactionStatus.PENDING;
        AccountNumber = accountNumber;
        Country = country;
    }

    public static AirtelPaymentTransaction CreatePayIn(int providerId, string userId, Money amount,
        string accountNumber,
        string transactionId, string country)
    {
        return new AirtelPaymentTransaction(providerId, userId, amount, TransactionType.PAYIN, accountNumber, transactionId,  country);
    }

    public static AirtelPaymentTransaction CreatePayOut(int providerId, string userId, Money amount,
        string accountNumber,
    string transactionId, string country)
    {
        return new AirtelPaymentTransaction(providerId, userId, amount, TransactionType.PAYOUT, accountNumber, transactionId, country);
    }

    public virtual Result SetInProgress()
    {
        //todo: check if status change is valid
        AirtelStatus = AirtelPaymentTransactionStatus.IN_PROGRESS;
        return base.SetInProgress();
    }

    public virtual Result Refund()
    {     
        AirtelStatus = AirtelPaymentTransactionStatus.REFUNDED;
        return base.Refund();
        
    }


    public new virtual Result SetFailed(string? error)
    {
        //todo: check if status change is valid
        if(base.Status != PaymentTransactionStatus.Failed)
        {
            AirtelStatus = AirtelPaymentTransactionStatus.FAILED;

        }
        return base.SetFailed(error);
    }

    public virtual Result Complete()
    {
        if (base.Status == PaymentTransactionStatus.InProgress)
        {
            AirtelStatus = AirtelPaymentTransactionStatus.COMPLETED;
            Status = PaymentTransactionStatus.Completed;
            return base.SetCompleted();
        }
        else
        {
            return base.SetAwaitingConfirmation();
        }
    }

    public new virtual Result SetUnconfirmed(string? error)
    {
        //todo: check if status change is valid
        AirtelStatus = AirtelPaymentTransactionStatus.UNCONFIRMED;
        Error = error;
        return base.SetAwaitingConfirmation();
    }

    public new virtual Result SetCanceled(string? error)
    {
        AirtelStatus = AirtelPaymentTransactionStatus.CANCELED;
        Status = PaymentTransactionStatus.Cancelled;
        return base.SetCancelled(error);
    }

    public virtual void SetAirtelTransactionId(string? airtelTransactionId)
    {
        AirtelTransactionId = airtelTransactionId;
    }




}