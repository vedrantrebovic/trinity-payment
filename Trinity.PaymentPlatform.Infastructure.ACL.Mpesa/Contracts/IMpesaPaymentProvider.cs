using FluentResults;
using OneOf;
using Trinity.PaymentPlatform.Infastructure.ACL.Mpesa.Models.Mpesa;
using Trinity.PaymentPlatform.Model.PaymentTransactionAggregate;
using Trinity.PaymentPlatform.Model.SharedKernel;

namespace Trinity.PaymentPlatform.Infastructure.ACL.Mpesa.Contracts;

public interface IMpesaPaymentProvider
{
    // Token
    Task<string> GetOAuthTokenAsync(string providerUrl, string key, string password);

    // Payin (STK Push)
    Task<OneOf<bool, IList<IDomainError>, Exception>> CreatePayinStkPushAsync(MpesaPayInRequest request) CreatePayinStkPushAsync(MpesaPayInRequest request); // create transaction 
    Task ConfirmPayinStkPushAsync(PaymentTransaction transaction, Model.PaymentProviderAggregate.PaymentProvider provider, string token); // send to provider payin
    Task ConfirmPayinAsync(MpesaCallbackRequest request); // callback payin 
    Task MPesaExpressQueryAsync(PaymentTransaction transaction, Model.PaymentProviderAggregate.PaymentProvider provider, string token); // status check payin 

    // Payout
    Task ProcessPayoutAsync(MpesaPayoutRequest request); // create transaction
    Task ConfirmPayoutAsync(PaymentTransaction transaction, Model.PaymentProviderAggregate.PaymentProvider provider, string token); // send to provider payou
    Task ProcessB2CResultAsync(B2CResultRequestPayout request); // callback payin
    Task TransactionStatusQueryAsync(PaymentTransaction transaction, Model.PaymentProviderAggregate.PaymentProvider provider, string token); // status check payou
    Task ProcessPayoutStatusCheckAsync(B2CResultRequestPayout request); // callback status check 
    Task ProcessTimeoutRequestAsync(B2CRequest request); // timeout callback payout
    Task ProcessStatusCheckTimeoutRequestAsync(TransactionStatusQueryRequest request); // timeout status check payout 

}