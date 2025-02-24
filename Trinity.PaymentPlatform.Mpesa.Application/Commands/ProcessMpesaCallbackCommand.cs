using Microsoft.Extensions.Logging;
using OneOf;
using OneOf.Types;
using Trinity.PaymentPlatform.Application.Shared.Contracts;
using Trinity.PaymentPlatform.Infrastructure.ACL.Mpesa.Contracts;
using Trinity.PaymentPlatform.Infrastructure.ACL.Mpesa.Models.Mpesa;
using Trinity.PaymentPlatform.Model.SeedWork;
using Trinity.PaymentPlatform.Model.SharedKernel;

namespace Trinity.PaymentPlatform.Mpesa.Application.Commands;

public record ProcessMpesaPayinCallbackCommand(MpesaCallbackRequest MpesaCallbackRequest):IBaseRequest<bool>;

public class ProcessMpesaPayinCallbackCommandHandler(ILogger<ProcessMpesaPayinCallbackCommandHandler> logger, IUnitOfWork unitOfWork, 
    IMpesaPaymentProvider mpesaPaymentProvider):IBaseRequestHandler<ProcessMpesaPayinCallbackCommand, bool>
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

            unitOfWork.BeginTransaction();

            var result = await mpesaPaymentProvider.ConfirmPayinAsync(request.MpesaCallbackRequest);

            await unitOfWork.CommitAsync();
            return result.IsSuccess;
        }
        catch (Exception e)
        {
            logger.LogError(e, e.Message);
            return e;
        }
        finally
        {
            await unitOfWork.RollbackAsync();
        }
    }
}