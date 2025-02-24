using Quartz;

namespace Trinity.PaymentPlatform.Processors.Outbox.Jobs;

[DisallowConcurrentExecution]
public class SendOutboxJob:IJob
{
    public Task Execute(IJobExecutionContext context)
    {
        throw new NotImplementedException();
    }
}