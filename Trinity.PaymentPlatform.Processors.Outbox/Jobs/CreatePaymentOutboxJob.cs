using Quartz;
using Trinity.PaymentPlatform.Model.PaymentTransactionAggregate;
using Trinity.PaymentPlatform.Model.PaymentTransactionOutboxAggregate;
using Trinity.PaymentPlatform.Model.SeedWork;

namespace Trinity.PaymentPlatform.Processors.Outbox.Jobs;

[DisallowConcurrentExecution]
public class CreatePaymentOutboxJob(ILogger<CreatePaymentOutboxJob> logger, IUnitOfWork unitOfWork, IPaymentTransactionRepository transactionRepository,
    IPaymentTransactionOutboxRepository outboxRepository, IConfiguration configuration):IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        try
        {
            //todo: take this from somewhere else
            string url = configuration["TransactionConfirmUrl"];

            unitOfWork.BeginTransaction();

            var transactions = await transactionRepository.GetFinalizedForOutbox();

            foreach (var paymentTransaction in transactions)
            {
                var outbox = PaymentTransactionOutbox.Create(paymentTransaction, url);
                await outboxRepository.SaveAsync(outbox);
                paymentTransaction.SetOutboxCreated();
                await transactionRepository.UpdateAsync(paymentTransaction);
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