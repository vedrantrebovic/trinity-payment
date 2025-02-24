using FluentResults;
using OneOf;
using Trinity.PaymentPlatform.Infrastructure.ACL.Mpesa.Models.Mpesa;
using Trinity.PaymentPlatform.Model.PaymentTransactionAggregate;
using Trinity.PaymentPlatform.Model.SharedKernel;

namespace Trinity.PaymentPlatform.Infrastructure.ACL.Mpesa.Contracts;

public interface IMpesaPaymentProvider
{
    // Payin (STK Push)
    Task<Result<long>> CreatePayinStkPushAsync(MpesaPayInRequest request);// create transaction 
    /// <summary>
    /// Sends the payin request to the provider
    /// </summary>
    /// <param name="transaction"></param>
    /// <param name="provider"></param>
    /// <returns></returns>
    Task<Result> ConfirmPayinStkPushAsync(MpesaPaymentTransaction transaction); // send to provider payin
    Task<Result> ConfirmPayinAsync(MpesaCallbackRequest request); // callback payin 
    Task<Result> MPesaExpressQueryAsync(MpesaPaymentTransaction transaction); // status check payin 

    // Payout
    Task<Result<long>> CreatePayoutAsync(MpesaPayoutRequest request); // create transaction
    Task<Result> ConfirmPayoutAsync(MpesaPaymentTransaction transaction); // send to provider payout
    Task<Result> ProcessB2CResultAsync(B2CResultRequestPayout request); // callback payin
    Task<Result> TransactionStatusQueryAsync(MpesaPaymentTransaction transaction); // status check payout
    Task<Result> ProcessPayoutStatusCheckAsync(B2CResultRequestPayout request); // callback status check 
    Task<Result> ProcessTimeoutRequestAsync(B2CRequest request); // timeout callback payout
    Task<Result> ProcessStatusCheckTimeoutRequestAsync(TransactionStatusQueryRequest request); // timeout status check payout 

}