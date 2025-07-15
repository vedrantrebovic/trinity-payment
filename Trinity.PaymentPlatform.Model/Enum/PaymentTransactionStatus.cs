namespace Trinity.PaymentPlatform.Model.Enum;

public enum PaymentTransactionStatus
{
    Pending = 0,
    InProgress = 1,
    AwaitingConfirmation = 2,

    //final states - no further state transitions
    Completed = 10,
    Rejected = 20,
    Cancelled =30,
    Failed = 40,
    Refunded = 50,
}