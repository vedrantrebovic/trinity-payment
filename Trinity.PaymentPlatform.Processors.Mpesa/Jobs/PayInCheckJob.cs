using Quartz;
using Trinity.PaymentPlatform.Infastructure.ACL.Mpesa.Contracts;
using Trinity.PaymentPlatform.Model.Enum;
using Trinity.PaymentPlatform.Model.PaymentTransactionAggregate;
using Trinity.PaymentPlatform.Model.SeedWork;

namespace Trinity.PaymentPlatform.Processors.Mpesa.Jobs;

[DisallowConcurrentExecution]
public class PayInCheckJob(ILogger<PayInCheckJob> logger, IServiceProvider serviceProvider) :IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        try
        {
            await using var outerScope = serviceProvider.CreateAsyncScope();
            var outerUnitOfWork = outerScope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var outerTransactionRepository = outerScope.ServiceProvider.GetRequiredService<IPaymentTransactionRepository>();

            var transactions = await outerTransactionRepository.GetTransactions(1, TransactionType.PAYIN, MpesaPaymentTransactionStatus.IN_PROGRESS, 5, 20);

            foreach (var transaction in transactions)
            {
                await using var innerScope = serviceProvider.CreateAsyncScope();
                var innerUnitOfWork = innerScope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var innerTransactionRepository = innerScope.ServiceProvider.GetRequiredService<IPaymentTransactionRepository>();
                var mpesaService = innerScope.ServiceProvider.GetRequiredService<IMpesaPaymentProvider>();
                
                
            }
        }
        catch (Exception e)
        {
            logger.LogError(e, e.Message);
        }
        finally
        {

        }
    }
}