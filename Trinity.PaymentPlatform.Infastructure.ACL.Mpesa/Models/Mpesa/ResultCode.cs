namespace Trinity.PaymentPlatform.Infrastructure.ACL.Mpesa.Models.Mpesa
{
    public enum ResultCode
    {
        Success = 0,
        InsufficientBalance = 1,
        InitiatorInfoInvalid = 2001,
        TransactionExpired = 1019,
        TransactionInProcess = 1001,
        PushRequestError = 1025,
        PushRequestErrorDuplicate = 9999,
        STKPushNotReached = 1037,
        STKPushNoResponse = 1037,
        RequestCancelledByUser = 1032
    }

}
