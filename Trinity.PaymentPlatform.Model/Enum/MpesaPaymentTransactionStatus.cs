namespace Trinity.PaymentPlatform.Model.Enum;

public enum MpesaPaymentTransactionStatus
{
    PENDING = 1,
    IN_PROGRESS = 2,
    FAILED = 3,
    COMPLETED = 4,
    CANCELED = 5,
    REFUNDED = 6,
    UNCONFIRMED = 7
}