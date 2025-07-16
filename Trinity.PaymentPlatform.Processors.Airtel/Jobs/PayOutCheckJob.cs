using Microsoft.Extensions.Options;
using Quartz;
using Trinity.PaymentPlatform.Infrastructure.ACL.Airtel;
using Trinity.PaymentPlatform.Infrastructure.ACL.Airtel.Contracts;
using Trinity.PaymentPlatform.Model.Enum;
using Trinity.PaymentPlatform.Model.PaymentTransactionAggregate;
using Trinity.PaymentPlatform.Model.SeedWork;

namespace Trinity.PaymentPlatform.Processors.Airtel.Jobs;

[DisallowConcurrentExecution]
public class PayOutCheckJob(ILogger<PayOutCheckJob> logger, IUnitOfWork unitOfWork, IPaymentTransactionRepository repository,
    IAirtelPaymentProvider paymentProvider, IOptions<AirtelConfig> airtelOptions) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        try
        {
            unitOfWork.BeginTransaction();

            var transactions = await repository.GetAirtelTransactions(2, TransactionType.PAYOUT,
                AirtelPaymentTransactionStatus.IN_PROGRESS, airtelOptions.Value.StatusCheckDelay, airtelOptions.Value.StatusCheckLimit);

            foreach (var transaction in transactions)
            {
                try
                {
                     await paymentProvider.ProcessStatusCheckPayoutAsync(transaction);
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