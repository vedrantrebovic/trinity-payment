using Quartz;
using Trinity.PaymentPlatform.Infrastructure.ACL.Mpesa.Contracts;
using Trinity.PaymentPlatform.Model.Enum;
using Trinity.PaymentPlatform.Model.PaymentTransactionAggregate;
using Trinity.PaymentPlatform.Model.SeedWork;

namespace Trinity.PaymentPlatform.Processors.Mpesa.Jobs;

[DisallowConcurrentExecution]
public class PayInCheckJob(ILogger<PayInCheckJob> logger, IUnitOfWork unitOfWork, IPaymentTransactionRepository repository,
    IMpesaPaymentProvider paymentProvider) :IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        try
        {
            unitOfWork.BeginTransaction();

            var transactions = await repository.GetTransactions(1, TransactionType.PAYIN,
                MpesaPaymentTransactionStatus.IN_PROGRESS, 5, 20);

            foreach (var transaction in transactions)
            {
                try
                {
                    await paymentProvider.MPesaExpressQueryAsync(transaction);
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