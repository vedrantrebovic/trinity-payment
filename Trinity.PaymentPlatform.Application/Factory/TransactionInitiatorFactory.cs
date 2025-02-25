using Microsoft.Extensions.DependencyInjection;
using Trinity.PaymentPlatform.Application.Models;
using Trinity.PaymentPlatform.Infrastructure.ACL.Mpesa.Services;
using Trinity.PaymentPlatform.Model.Contracts;

namespace Trinity.PaymentPlatform.Application.Factory;

public class TransactionInitiatorFactory(IServiceProvider provider): ITransactionInitiatorFactory
{
    public ITransactionInitiator GetTransactionInitiator(int id)
    {
        switch (id)
        {
            case 1: return provider.GetRequiredKeyedService<ITransactionInitiator>("MpesaTransactionInitiator");
            default: throw new NotSupportedException("Payment provider not supported");
        }
    }
}

public class PayInTransactionInitiationParamsConverter: IPayInTransactionInitiationParamsConverter
{

    public IPayInTransactionInitiationParams? Convert(int id, IPayInTransactionInputParams input)
    {
        switch (id)
        {
            case 1: 
                var mpesaModel = input as MpesaPayInModel;
                if (mpesaModel == null) return null;
                return new MpesaPayInTransactionInitiationParams(mpesaModel.UserId, mpesaModel.Amount, mpesaModel.CurrencyCode, mpesaModel.PhoneNumber, mpesaModel.TransactionReference);

            default: throw new NotSupportedException("Payment provider not supported");
        }
    }
}

public class PayoutTransactionInitiationParamsConverter: IPayoutTransactionInitiationParamsConverter
{
    public IPayoutTransactionInitiationParams? Convert(int id, IPayoutTransactionInputParams input)
    {
        switch (id)
        {
            case 1:
                var mpesaModel = input as MpesaPayoutModel;
                if (mpesaModel == null) return null;
                return new MpesaPayoutTransactionInitiationParams(mpesaModel.UserId, mpesaModel.Amount, mpesaModel.CurrencyCode, mpesaModel.PhoneNumber, mpesaModel.TransactionReference);
            default: throw new NotSupportedException("Payment provider not supported");
        }
    }
}