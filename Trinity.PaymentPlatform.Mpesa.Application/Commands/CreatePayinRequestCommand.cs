using Microsoft.Extensions.Logging;
using OneOf;
using OneOf.Types;
using Trinity.PaymentPlatform.Application.Shared.Contracts;
using Trinity.PaymentPlatform.Infrastructure.ACL.Mpesa.Contracts;
using Trinity.PaymentPlatform.Infrastructure.ACL.Mpesa.Models.Mpesa;
using Trinity.PaymentPlatform.Model.SeedWork;
using Trinity.PaymentPlatform.Model.SharedKernel;

namespace Trinity.PaymentPlatform.Mpesa.Application.Commands;

public record CreatePayinRequestCommand(string UserId, decimal Amount, string CurrencyCode, string AccountNumber, string TransactionReference) :IBaseRequest<long>;

public class CreatePayinRequestCommandHandler(ILogger<CreatePayinRequestCommandHandler> logger, IMpesaPaymentProvider paymentProvider) :IBaseRequestHandler<CreatePayinRequestCommand, long>
{
    public async Task<OneOf<long, None, List<IDomainError>, Exception>> Handle(CreatePayinRequestCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await paymentProvider.CreatePayinStkPushAsync(new MpesaPayInRequest(request.UserId,
                request.Amount, request.CurrencyCode, request.AccountNumber, request.TransactionReference));
            if (result.IsFailed)
                return DomainError.CreateList(result.Errors);

            return result.Value;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}