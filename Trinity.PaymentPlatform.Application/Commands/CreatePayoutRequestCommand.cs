using Microsoft.Extensions.Logging;
using OneOf;
using OneOf.Types;
using Trinity.PaymentPlatform.Application.Models;
using Trinity.PaymentPlatform.Application.Shared.Contracts;
using Trinity.PaymentPlatform.Model.Contracts;
using Trinity.PaymentPlatform.Model.SeedWork;
using Trinity.PaymentPlatform.Model.SharedKernel;

namespace Trinity.PaymentPlatform.Application.Commands;

public record CreatePayoutRequestCommand(int ProviderId, PayoutModel Model):IBaseRequest<long>;

public class CreatePayoutRequestCommandHandler(ILogger<CreatePayoutRequestCommandHandler> logger, IUnitOfWork unitOfWork, ITransactionInitiatorFactory transactionInitiatorFactory,
    IPayoutTransactionInitiationParamsConverter payoutTransactionParamsConverter) : IBaseRequestHandler<CreatePayoutRequestCommand, long>
{
    public async Task<OneOf<long, None, List<IDomainError>, Exception>> Handle(CreatePayoutRequestCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var transactionInitiator = transactionInitiatorFactory.GetTransactionInitiator(request.ProviderId);
            var input = payoutTransactionParamsConverter.Convert(request.ProviderId, request.Model);
            if (input == null)
                return DomainError.Unspecified("invalid_input_data");

            unitOfWork.BeginTransaction();

            //var model = request.Model as MpesaPayoutModel;

            var result = await transactionInitiator.CreatePayout(input);
            if (result.IsFailed)
                return DomainError.CreateList(result.Errors);

          
                
                await unitOfWork.CommitAsync();
          
            var finalValue = result.Value;
            Console.WriteLine("Returning value: " + finalValue);
            return finalValue;

        }
        catch (Exception ex)
        {
            try
            {
                Console.WriteLine("EXCEPTION CAUGHT:");
                Console.WriteLine(ex.ToString());
                Console.WriteLine("Inner: " + ex.InnerException?.ToString());
            }
            catch (Exception e) {

                Console.WriteLine(e.ToString());

            }

            logger.LogError(ex, "Commit succeeded but exception caught after. Message: {Message}", ex.Message);
            return ex;
        }
        finally
        {
            await unitOfWork.RollbackAsync();
        }
    }
}