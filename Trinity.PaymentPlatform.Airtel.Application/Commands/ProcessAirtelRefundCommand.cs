using Microsoft.Extensions.Logging;
using OneOf;
using OneOf.Types;
using Trinity.PaymentPlatform.Application.Shared.Contracts;
using Trinity.PaymentPlatform.Infrastructure.ACL.Airtel.Contracts;
using Trinity.PaymentPlatform.Infrastructure.ACL.Airtel.Model;
using Trinity.PaymentPlatform.Model.SeedWork;
using Trinity.PaymentPlatform.Model.SharedKernel;

namespace Trinity.PaymentPlatform.Airtel.Application.Commands;

public record ProcessAirtelRefundCommand(AirtelRefundRequest AirtelRefundRequest) : IBaseRequest<bool>;

public class ProcessAirtelRefundCommandHandler(ILogger<ProcessAirtelRefundCommandHandler> logger, IUnitOfWork unitOfWork,
    IAirtelPaymentProvider airtelPaymentProvider) : IBaseRequestHandler<ProcessAirtelRefundCommand, bool>
{
    public async Task<OneOf<bool, None, List<IDomainError>, Exception>> Handle(ProcessAirtelRefundCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (request.AirtelRefundRequest.TransactionReference == null)
            {
                logger.LogError($"Invalid refund request");
                return DomainError.Unspecified("refund_request_invalid");
            }

            unitOfWork.BeginTransaction();

            var result = await airtelPaymentProvider.ProcessRefundAsync(request.AirtelRefundRequest);

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