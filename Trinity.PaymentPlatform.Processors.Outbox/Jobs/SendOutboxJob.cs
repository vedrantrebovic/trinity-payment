using System.Net.Http.Json;
using Quartz;
using Trinity.PaymentPlatform.Model.PaymentTransactionOutboxAggregate;
using Trinity.PaymentPlatform.Model.SeedWork;
using Trinity.PaymentPlatform.Processors.Outbox.Models;

namespace Trinity.PaymentPlatform.Processors.Outbox.Jobs;

[DisallowConcurrentExecution]
public class SendOutboxJob(ILogger<SendOutboxJob> logger, IUnitOfWork unitOfWork, IPaymentTransactionOutboxRepository outboxRepository, IHttpClientFactory httpClientFactory):IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        try
        {
            var httpClient = httpClientFactory.CreateClient();

            unitOfWork.BeginTransaction();

            var outboxItems = await outboxRepository.GetUnsent();

            foreach (var item in outboxItems)
            {
                try
                {
                    var result = await httpClient.PostAsJsonAsync(item.CallbackUrl, new TransactionConfirmationModel(
                        item.TransactionReference, item.TransactionReference,
                        item.Amount.Amount, item.Amount.CurrencyCode, item.Success, item.Error));
                    if (result.IsSuccessStatusCode)
                    {
                        item.MarkAsSent();
                        await outboxRepository.UpdateAsync(item);
                    }
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