using Microsoft.Extensions.Logging;
using OneOf;
using OneOf.Types;
using Trinity.PaymentPlatform.Application.Shared.Contracts;
using Trinity.PaymentPlatform.Infrastructure.ACL.Airtel.Contracts;
using Trinity.PaymentPlatform.Infrastructure.ACL.Airtel.Model;
using Trinity.PaymentPlatform.Model.SeedWork;
using Trinity.PaymentPlatform.Model.SharedKernel;

namespace Trinity.PaymentPlatform.Airtel.Application.Commands;

public record ProcessAirtelPayinCallbackCommand(AirtelCallbackRequest AirtelCallbackRequest):IBaseRequest<bool>;

public class ProcessAirtelPayinCallbackCommandHandler(ILogger<ProcessAirtelPayinCallbackCommandHandler> logger, IUnitOfWork unitOfWork, 
    IAirtelPaymentProvider airtelPaymentProvider):IBaseRequestHandler<ProcessAirtelPayinCallbackCommand, bool>
{
    public async Task<OneOf<bool, None, List<IDomainError>, Exception>> Handle(ProcessAirtelPayinCallbackCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (request.AirtelCallbackRequest.Transaction == null)
            {
                logger.LogError($"Invalid Airtel payin callback received {request}");
                return DomainError.Unspecified("callback_request_invalid");
            }

            unitOfWork.BeginTransaction();

            var result = await airtelPaymentProvider.ProcessCallbackPayinAsync(request.AirtelCallbackRequest);

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