using Quartz;
using Trinity.PaymentPlatform.Infrastructure.ACL.Mpesa.Contracts;
using Trinity.PaymentPlatform.Model.Enum;
using Trinity.PaymentPlatform.Model.PaymentTransactionAggregate;
using Trinity.PaymentPlatform.Model.SeedWork;

namespace Trinity.PaymentPlatform.Processors.Mpesa.Jobs;

[DisallowConcurrentExecution]
public class ProcessPayInTransactionJob(ILogger<ProcessPayInTransactionJob> logger, IUnitOfWork unitOfWork, IPaymentTransactionRepository repository,
    IMpesaPaymentProvider paymentProvider) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        try
        {
            unitOfWork.BeginTransaction();

            var transactions = await repository.GetTransactions(1, TransactionType.PAYIN, MpesaPaymentTransactionStatus.PENDING, 0, 20);

            foreach (var transaction in transactions)
            {
                try
                {
                    // Process transaction
                    await paymentProvider.ConfirmPayinStkPushAsync(transaction);

                    //todo: create outbox transaction if needed
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