using Microsoft.Extensions.Logging;
using OneOf;
using OneOf.Types;
using Trinity.PaymentPlatform.Application.Shared.Contracts;
using Trinity.PaymentPlatform.Infrastructure.ACL.Mpesa.Contracts;
using Trinity.PaymentPlatform.Infrastructure.ACL.Mpesa.Models.Mpesa;
using Trinity.PaymentPlatform.Model.SeedWork;
using Trinity.PaymentPlatform.Model.SharedKernel;

namespace Trinity.PaymentPlatform.Mpesa.Application.Commands;

public record ProcessTimeoutRequestCommand(B2CRequest MpesaRequest):IBaseRequest<bool>;

public class ProcessTimeoutRequestCommandHandler(ILogger<ProcessTimeoutRequestCommandHandler> logger, IUnitOfWork unitOfWork, IMpesaPaymentProvider paymentProvider) 
    : IBaseRequestHandler<ProcessTimeoutRequestCommand, bool>
{
    public async Task<OneOf<bool, None, List<IDomainError>, Exception>> Handle(ProcessTimeoutRequestCommand request, CancellationToken cancellationToken)
    {
        try
        {
            unitOfWork.BeginTransaction();
            var result = await paymentProvider.ProcessTimeoutRequestAsync(request.MpesaRequest);

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