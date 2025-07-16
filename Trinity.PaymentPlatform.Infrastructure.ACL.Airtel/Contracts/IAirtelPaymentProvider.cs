using FluentResults;
using OneOf;
using Trinity.PaymentPlatform.Infrastructure.ACL.Airtel.Model;
using Trinity.PaymentPlatform.Model.PaymentTransactionAggregate;
using Trinity.PaymentPlatform.Model.SharedKernel;

namespace Trinity.PaymentPlatform.Infrastructure.ACL.Airtel.Contracts;

public interface IAirtelPaymentProvider
{
    // Payin 
    Task<Result<long>> CreatePayinAsync(AirtelPayInRequest request);// create transaction 
    Task ConfirmPayinAsync(AirtelPaymentTransaction transaction); // send to provider payin
    Task<Result> ProcessCallbackPayinAsync(AirtelCallbackRequest request); // callback payin 
    Task ProcessStatusCheckPayinAsync(AirtelPaymentTransaction transaction); // status check payin 
    Task<Result> ProcessRefundAsync(AirtelRefundRequest request); // refund  


    // Payout
    Task<Result<long>> CreatePayoutAsync(AirtelPayoutRequest request); // create transaction
    Task ConfirmPayoutAsync(AirtelPaymentTransaction transaction); // send to provider payout
    Task ProcessStatusCheckPayoutAsync(AirtelPaymentTransaction request); // status check payout 


}