using FluentResults;
using Microsoft.Extensions.Logging;
using Trinity.PaymentPlatform.Model.Contracts;
using Trinity.PaymentPlatform.Model.PaymentProviderAggregate;
using Trinity.PaymentPlatform.Model.PaymentTransactionAggregate;
using Trinity.PaymentPlatform.Model.SharedKernel;
using Trinity.PaymentPlatform.Model.Util;

namespace Trinity.PaymentPlatform.Infrastructure.ACL.Mpesa.Services;

public class MpesaTransactionInitiator(ILogger<MpesaTransactionInitiator> logger, IPaymentTransactionRepository transactionRepository,
    IPaymentProviderRepository paymentProviderRepository) :ITransactionInitiator
{
    private const int ProviderId = 1;
    public const string Name = "MpesaTransactionInitiator";

    public async Task<Result<long>> CreatePayIn(IPayInTransactionInitiationParams parameters)
    {
        try
        {
            var request = parameters as MpesaPayInTransactionInitiationParams;
            if (request == null)
                return Result.Fail(ErrorMessageFormatter.Error("invalid_parameters"));

            var existing = await transactionRepository.GetByTransactionIdAsync(request.TransactionReference);
            if (existing != null) return existing.Id;

            var provider = await paymentProviderRepository.GetAsync(ProviderId);
            if (provider == null)
            {
                logger.LogError("Mpesa: Provider {ProviderId} not found", ProviderId);
                return ErrorMessageFormatter.FailWithError("payment_provider_not_found");
            }

            var moneyAmount = Money.Create(request.Amount, request.CurrencyCode); //todo: check if valid currency code
            if (moneyAmount.IsFailed)
                return moneyAmount.ToResult();

            var transaction = MpesaPaymentTransaction.CreatePayIn(ProviderId, request.UserId,
                moneyAmount.Value, request.AccountNumber, request.TransactionReference);

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
            var request = parameters as MpesaPayoutTransactionInitiationParams;
            if (request == null)
                return Result.Fail(ErrorMessageFormatter.Error("invalid_parameters"));

            var existing = await transactionRepository.GetByTransactionIdAsync(request.TransactionReference);
            if (existing != null) return existing.Id;

            var provider = await paymentProviderRepository.GetAsync(ProviderId);
            if (provider == null)
            {
                logger.LogError("Mpesa: Provider {ProviderId} not found", ProviderId);
                return ErrorMessageFormatter.FailWithError("payment_provider_not_found");
            }

            var moneyAmount = Money.Create(request.Amount, request.CurrencyCode); //todo: check if valid currency code

            var transaction = MpesaPaymentTransaction.CreatePayOut(ProviderId, request.UserId,
                moneyAmount.Value,
                request.AccountNumber, request.TransactionReference);

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

public record MpesaPayInTransactionInitiationParams(
    string UserId,
    decimal Amount,
    string CurrencyCode,
    string AccountNumber,
    string TransactionReference) : IPayInTransactionInitiationParams;
public record MpesaPayoutTransactionInitiationParams(string UserId,
    decimal Amount,
    string CurrencyCode,
    string AccountNumber,
    string TransactionReference) : IPayoutTransactionInitiationParams;