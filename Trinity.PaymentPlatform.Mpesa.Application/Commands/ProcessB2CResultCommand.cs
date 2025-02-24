using Microsoft.Extensions.Logging;
using OneOf;
using OneOf.Types;
using Trinity.PaymentPlatform.Application.Shared.Contracts;
using Trinity.PaymentPlatform.Infrastructure.ACL.Mpesa.Contracts;
using Trinity.PaymentPlatform.Infrastructure.ACL.Mpesa.Models.Mpesa;
using Trinity.PaymentPlatform.Model.SeedWork;
using Trinity.PaymentPlatform.Model.SharedKernel;

namespace Trinity.PaymentPlatform.Mpesa.Application.Commands;

public record ProcessB2CResultCommand(B2CResultRequestPayout MpesaRequest):IBaseRequest<bool>;

public class ProcessB2CResultCommandHandler(ILogger<ProcessB2CResultCommandHandler> logger, IUnitOfWork unitOfWork,
    IMpesaPaymentProvider mpesaPaymentProvider) : IBaseRequestHandler<ProcessB2CResultCommand, bool>
{
    public async Task<OneOf<bool, None, List<IDomainError>, Exception>> Handle(ProcessB2CResultCommand request, CancellationToken cancellationToken)
    {
        try
        {
            unitOfWork.BeginTransaction();

            var result = await mpesaPaymentProvider.ProcessB2CResultAsync(request.MpesaRequest);

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