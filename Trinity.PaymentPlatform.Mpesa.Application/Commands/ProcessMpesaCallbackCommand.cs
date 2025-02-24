using Microsoft.Extensions.Logging;
using OneOf;
using OneOf.Types;
using Trinity.PaymentPlatform.Application.Shared.Contracts;
using Trinity.PaymentPlatform.Infrastructure.ACL.Mpesa.Contracts;
using Trinity.PaymentPlatform.Infrastructure.ACL.Mpesa.Models.Mpesa;
using Trinity.PaymentPlatform.Model.SharedKernel;

namespace Trinity.PaymentPlatform.Mpesa.Application.Commands;

public record ProcessMpesaPayinCallbackCommand(MpesaCallbackRequest MpesaCallbackRequest):IBaseRequest<bool>;

public class ProcessMpesaPayinCallbackCommandHandler(ILogger<ProcessMpesaPayinCallbackCommandHandler> logger, IMpesaPaymentProvider mpesaPaymentProvider):IBaseRequestHandler<ProcessMpesaPayinCallbackCommand, bool>
{
    public async Task<OneOf<bool, None, List<IDomainError>, Exception>> Handle(ProcessMpesaPayinCallbackCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (request.MpesaCallbackRequest?.Body.StkCallback == null)
            {
                logger.LogError($"Invalid Mpesa payin callback received {request}");
                return DomainError.Unspecified("callback_request_invalid");
            }

            var result = await mpesaPaymentProvider.ConfirmPayinAsync(request.MpesaCallbackRequest);
            return result.IsSuccess;
        }
        catch (Exception e)
        {
            logger.LogError(e, e.Message);
            return e;
        }
    }
}