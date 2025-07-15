using FluentResults;
using Microsoft.Extensions.Logging;
using Trinity.PaymentPlatform.Model.Contracts;
using Trinity.PaymentPlatform.Model.PaymentProviderAggregate;
using Trinity.PaymentPlatform.Model.PaymentTransactionAggregate;
using Trinity.PaymentPlatform.Model.SharedKernel;
using Trinity.PaymentPlatform.Model.Util;

namespace Trinity.PaymentPlatform.Infrastructure.ACL.Airtel.Services;

public class AirtelTransactionInitiator(ILogger<AirtelTransactionInitiator> logger, IPaymentTransactionRepository transactionRepository,
    IPaymentProviderRepository paymentProviderRepository) :ITransactionInitiator
{
    private const int ProviderId = 2;
    public const string Name = "AirtelTransactionInitiator";

    public async Task<Result<long>> CreatePayIn(IPayInTransactionInitiationParams parameters)
    {
        try
        {
            var request = parameters as AirtelPayInTransactionInitiationParams;
            if (request == null)
                return Result.Fail(ErrorMessageFormatter.Error("invalid_parameters"));

            var existing = await transactionRepository.GetByTransactionIdAsync(request.TransactionReference);
            if (existing != null) return existing.Id;

            var provider = await paymentProviderRepository.GetAsync(ProviderId);
            if (provider == null)
            {
                logger.LogError("Airtel: Provider {ProviderId} not found", ProviderId);
                return ErrorMessageFormatter.FailWithError("payment_provider_not_found");
            }

            var moneyAmount = Money.Create(request.Amount, request.CurrencyCode); //todo: check if valid currency code
            if (moneyAmount.IsFailed)
                return moneyAmount.ToResult();

            var transaction = AirtelPaymentTransaction.CreatePayIn(ProviderId, request.UserId,
                moneyAmount.Value, request.AccountNumber, request.TransactionReference, request.Country);

            await transactionRepository.SaveAsync(transaction);

            return transaction.Id;
        }
        catch (Exception e)
        {
            logger.LogError(e, e.Message);
            return ErrorMessageFormatter.FailWithError();
        }
    }

    public async Task<Result<long>> CreatePayout(IPayoutTransactionInitiationParams parameters)
    {
        try
        {
            var request = parameters as AirtelPayoutTransactionInitiationParams;
            if (request == null)
                return Result.Fail(ErrorMessageFormatter.Error("invalid_parameters"));

            var existing = await transactionRepository.GetByTransactionIdAsync(request.TransactionReference);
            if (existing != null) return existing.Id;

            var provider = await paymentProviderRepository.GetAsync(ProviderId);
            if (provider == null)
            {
                logger.LogError("Airtel: Provider {ProviderId} not found", ProviderId);
                return ErrorMessageFormatter.FailWithError("payment_provider_not_found");
            }

            var moneyAmount = Money.Create(request.Amount, request.CurrencyCode); //todo: check if valid currency code

            var transaction = AirtelPaymentTransaction.CreatePayOut(ProviderId, request.UserId,
                moneyAmount.Value,
                request.AccountNumber, request.TransactionReference, request.Country);

            await transactionRepository.SaveAsync(transaction);

            return transaction.Id;
        }
        catch (Exception e)
        {
            logger.LogError(e, e.Message);
            return ErrorMessageFormatter.FailWithError();
        }
    }
}

public record AirtelPayInTransactionInitiationParams(
    string UserId,
    decimal Amount,
    string CurrencyCode,
    string AccountNumber,
    string TransactionReference, string Country) : IPayInTransactionInitiationParams;
public record AirtelPayoutTransactionInitiationParams(string UserId,
    decimal Amount,
    string CurrencyCode,
    string AccountNumber,
    string TransactionReference, string Country) : IPayoutTransactionInitiationParams;