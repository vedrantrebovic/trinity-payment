using Quartz;
using Trinity.PaymentPlatform.Infrastructure.ACL.Airtel.Contracts;
using Trinity.PaymentPlatform.Model.Enum;
using Trinity.PaymentPlatform.Model.PaymentTransactionAggregate;
using Trinity.PaymentPlatform.Model.SeedWork;

namespace Trinity.PaymentPlatform.Processors.Airtel.Jobs;

[DisallowConcurrentExecution]
public class ProcessPayInTransactionJob(ILogger<ProcessPayInTransactionJob> logger, IUnitOfWork unitOfWork, IPaymentTransactionRepository repository, 
    IAirtelPaymentProvider paymentProvider) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        try
        {
            unitOfWork.BeginTransaction();

            var transactions = await repository.GetAirtelTransactions(2, TransactionType.PAYIN, AirtelPaymentTransactionStatus.PENDING, 0, 20);

            foreach (var transaction in transactions)
            {
                try
                {
                    // Process transaction
                    await paymentProvider.ConfirmPayinAsync(transaction);

                    await repository.UpdateAsync(transaction);
                }
                catch (Exception e)
                {
                    logger.LogError(e, e.Message);
                }
            }

            await unitOfWork.CommitAsync();
        }
        catch (Exception e)
        {
            logger.LogError(e, e.Message);
        }
        finally
        {
            await unitOfWork.RollbackAsync();
        }
    }
}