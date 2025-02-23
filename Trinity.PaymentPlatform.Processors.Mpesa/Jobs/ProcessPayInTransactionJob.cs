using Quartz;
using Trinity.PaymentPlatform.Model.SeedWork;

namespace Trinity.PaymentPlatform.Processors.Mpesa.Jobs;

[DisallowConcurrentExecution]
public class ProcessPayInTransactionJob(ILogger<ProcessPayInTransactionJob> logger, IUnitOfWork unitOfWork) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        logger.LogInformation("ProcessPayInTransactionJob");
    }
}