using System.Text;
using System.Text.Json;
using FluentResults;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Trinity.PaymentPlatform.Infrastructure.ACL.Mpesa.Contracts;
using Trinity.PaymentPlatform.Infrastructure.ACL.Mpesa.Models.Mpesa;
using Trinity.PaymentPlatform.Model.Enum;
using Trinity.PaymentPlatform.Model.PaymentProviderAggregate;
using Trinity.PaymentPlatform.Model.PaymentTransactionAggregate;
using Trinity.PaymentPlatform.Model.PaymentTransactionOutboxAggregate;
using Trinity.PaymentPlatform.Model.SharedKernel;
using Trinity.PaymentPlatform.Model.Util;

namespace Trinity.PaymentPlatform.Infrastructure.ACL.Mpesa.Services;


public class MpesaPaymentProvider(ILogger<MpesaPaymentProvider> logger, IPaymentTransactionRepository transactionRepository, IPaymentProviderRepository paymentProviderRepository,
    IPaymentTransactionOutboxRepository outboxRepository, IOptions<MpesaConfig> mpesaConfig, IHttpClientFactory httpClientFactory) : IMpesaPaymentProvider
{
    private readonly int _providerId = 1;
    private readonly MpesaConfig _mpesaConfig = mpesaConfig.Value;
    private readonly HttpClient _httpClient = httpClientFactory.CreateClient("Mpesa");

    #region Payin (STK Push)


    public async Task<Result<long>> CreatePayinStkPushAsync(MpesaPayInRequest request)
    {
        try
        {
            var existing = await transactionRepository.GetByTransactionIdAsync(request.TransactionReference);
            if (existing != null) return existing.Id;

            var provider = await paymentProviderRepository.GetAsync(_providerId);
            if (provider == null)
            {
                logger.LogError("Mpesa: Provider {ProviderId} not found", _providerId);
                return ErrorMessageFormatter.FailWithError("payment_provider_not_found");
            }

            var moneyAmount = Money.Create(request.Amount, request.CurrencyCode); //todo: check if valid currency code
            if (moneyAmount.IsFailed)
                return moneyAmount.ToResult();

            var transaction = MpesaPaymentTransaction.CreatePayIn(_providerId, request.UserId.ToString(),
                moneyAmount.Value, request.AccountNumber, request.TransactionReference);

            await transactionRepository.SaveAsync(transaction);

            return transaction.Id;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
            return ErrorMessageFormatter.FailWithError();
        }
    }

    public async Task ConfirmPayinStkPushAsync(MpesaPaymentTransaction transaction)
    {
        string timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        transaction.SetProviderTimestamp(timestamp);

        try
        {
            JsonDocument? responseDoc = null;

            var provider = await paymentProviderRepository.GetAsync(transaction.ProviderId);

            try
            {
                var requestData = new
                {
                    _mpesaConfig.BusinessShortCode,
                    Password = MpesaSecurity.GeneratePassword(_mpesaConfig.BusinessShortCode,
                        _mpesaConfig.PayinStkPushKey, timestamp),
                    Timestamp = timestamp,
                    TransactionType = _mpesaConfig.PayinStkPushTransactionType,
                    transaction.Amount.Amount,
                    PartyA = transaction.AccountNumber,
                    PartyB = _mpesaConfig.BusinessShortCode,
                    PhoneNumber = transaction.AccountNumber,
                    CallBackURL = $"{provider.CallbackUrl}/api/payin/mpesa/callback",
                    AccountReference =
                        $"PAYIN_{transaction.AccountNumber}", // Account Reference: This is an Alpha-Numeric parameter that is defined by your system as an Identifier of the transaction for the CustomerPayBillOnline transaction type. Along with the business name, this value is also displayed to the customer in the STK Pin Prompt message. Maximum of 12 characters.
                    TransactionDesc =
                        "Payin" // This is any additional information / comment that can be sent along with the request from your system.Maximum of 13 Characters.
                };

                var sendRequestResult = await SendMpesaRequestAsync("mpesa/stkpush/v1/processrequest",
                    requestData);

                if (sendRequestResult.IsSuccess)
                    responseDoc = sendRequestResult.Value;
                else
                {
                    transaction.SetFailed(string.Join('|', sendRequestResult.Errors.ConvertAll(p => p.Message)));
                    return;
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                transaction.SetUnconfirmed("Unhandled exception");
                return;
            }

            if (responseDoc != null)
            {
                using (responseDoc)
                {
                    string responseCode = responseDoc.RootElement.GetProperty("ResponseCode").GetString();
                    if (responseCode == "0")
                    {
                        transaction.SetRequestId(responseDoc.RootElement.GetProperty("MerchantRequestID").GetString(),
                            responseDoc.RootElement.GetProperty("CheckoutRequestID").GetString());
                        transaction.SetInProgress();
                    }
                    else
                    {
                        string resultDesc = responseDoc.RootElement.GetProperty("ResponseDescription").GetString();
                        transaction.SetFailed(resultDesc);
                        logger.LogError("Mpesa: Transaction {TransactionId} failed with response code {ResponseCode}",
                            transaction.TransactionId, responseCode);
                    }
                }
            }
        }

        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
            throw;
        }
    }

    public async Task<Result> ConfirmPayinAsync(MpesaCallbackRequest request)
    {
        try
        {
            var transaction = await transactionRepository.GetTransactionByMerchantIdAsync(
                request.Body.StkCallback.MerchantRequestID,
                request.Body.StkCallback.CheckoutRequestID);

            if (transaction == null || transaction.MpesaStatus != MpesaPaymentTransactionStatus.IN_PROGRESS)
            {
                logger.LogError("Mpesa: Transaction {MerchantRequestID} not found or not in progress",
                    request.Body.StkCallback.MerchantRequestID);
                return Result.Fail(ErrorMessageFormatter.Error("transaction_not_found"));
            }

            if (request.Body.StkCallback.ResultCode == (int)ResultCode.Success)
            {
                decimal amount = request.Body.StkCallback.CallbackMetadata.Item.GetValue("Amount", 0m);

                var validateData = MpesaSecurity.Validate(transaction, amount);
                if (validateData.IsSuccess)
                {
                    transaction.Complete();
                }
                else
                {
                    transaction.SetUnconfirmed(validateData.Value);
                    logger.LogWarning("Transaction {TransactionId} validation failed: {ValidateData}",
                        transaction.TransactionId, validateData.Value);
                }
            }
            else
            {
                transaction.SetFailed(request.Body.StkCallback.ResultDesc);
                logger.LogWarning("Transaction {TransactionId} failed", transaction.TransactionId);
            }

            await transactionRepository.UpdateAsync(transaction);

            return Result.Ok();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
            return Result.Fail(ErrorMessageFormatter.Error());
        }
    }

    public async Task MPesaExpressQueryAsync(MpesaPaymentTransaction transaction)
    {
        try
        {
            var requestData = new
            {
                _mpesaConfig.BusinessShortCode,
                Password = MpesaSecurity.GeneratePassword(_mpesaConfig.BusinessShortCode, _mpesaConfig.PayinStkPushKey, transaction.ProviderTimestamp),
                Timestamp = transaction.ProviderTimestamp,
                CheckoutRequestID = transaction.CheckoutRequestId
            };

            var responseDoc = await SendMpesaRequestAsync("mpesa/stkpushquery/v1/query", requestData);
            if (responseDoc.IsSuccess)
            {
                var content = responseDoc.Value.RootElement.GetRawText();
                var expressResponse = JsonSerializer.Deserialize<ExpressResponse>(content);
                if (expressResponse.ResultCode == "0")
                {
                    transaction.Complete();
                    return;
                }
                else
                {
                    string resultDesc = responseDoc.Value.RootElement.GetProperty("ResponseDescription").GetString();
                    transaction.SetFailed(resultDesc);
                    logger.LogError($"Mpesa: Transaction {transaction.TransactionId} failed with response {resultDesc}");
                    return;
                }
            }
            else
            {
                transaction.SetFailed(string.Join('|', responseDoc.Errors.ConvertAll(p => p.Message)));
                return;
            }

        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
            throw;
        }
    }

    #endregion

    #region Payout

    public async Task<Result<long>> CreatePayoutAsync(MpesaPayoutRequest request)
    {
        try
        {
            var existing = await transactionRepository.GetByTransactionIdAsync(request.TransactionReference);
            if (existing != null) return existing.Id;

            var provider = await paymentProviderRepository.GetAsync(_providerId);
            if (provider == null)
            {
                logger.LogError("Mpesa: Provider {ProviderId} not found", _providerId);
                return ErrorMessageFormatter.FailWithError("payment_provider_not_found");
            }

            var moneyAmount = Money.Create(request.Amount, request.CurrencyCode); //todo: check if valid currency code

            var transaction = MpesaPaymentTransaction.CreatePayOut(_providerId, request.UserId.ToString(),
                moneyAmount.Value,
                request.AccountNumber, request.TransactionReference);

            await transactionRepository.SaveAsync(transaction);

            return transaction.Id;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
            return ErrorMessageFormatter.FailWithError();
        }
    }

    public async Task ConfirmPayoutAsync(MpesaPaymentTransaction transaction)
    {
        try
        {
            JsonDocument? responseDoc = null;

            var provider = await paymentProviderRepository.GetAsync(_providerId);
            try
            {
                var requestData = new B2CRequest
                {
                    OriginatorConversationID = transaction.TransactionId,
                    InitiatorName = _mpesaConfig.InitiatorName,
                    SecurityCredential = MpesaSecurity.GenerateSecurityCredential(_mpesaConfig.InitiatorPassword, _mpesaConfig.CertificateName),
                    CommandID = _mpesaConfig.PayoutCommandID,
                    Amount = transaction.Amount.Amount,
                    PartyA = _mpesaConfig.MerchantId,
                    PartyB = transaction.AccountNumber,
                    Remarks = $"{transaction.TransactionId}_{transaction.AccountNumber}_{transaction.Amount}",
                    QueueTimeOutURL = $"{provider.CallbackUrl}/api/payout/timeout",
                    ResultURL = $"{provider.CallbackUrl}/api/payout/result"
                };

                var responseDocResult = await SendMpesaRequestAsync($"mpesa/b2c/v3/paymentrequest", requestData);
                if (responseDocResult.IsSuccess)
                    responseDoc = responseDocResult.Value;
                else
                {
                    transaction.SetFailed(string.Join('|', responseDocResult.Errors.ConvertAll(p => p.Message)));
                    return;
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                transaction.SetUnconfirmed("Unhandled exception");
                return;
            }

            if (responseDoc != null)
            {
                using (responseDoc)
                {
                    if (responseDoc.RootElement.TryGetProperty("ConversationID", out var convId))
                    {
                        transaction.SetInProgress(convId.GetString());
                    }
                    else
                    {
                        transaction.SetFailed("Response did not contain ConversationID");
                        logger.LogError("ConfirmPayoutAsync failed: Response did not contain ConversationID.");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
            throw;
        }
    }

    public async Task<Result> ProcessB2CResultAsync(B2CResultRequestPayout request)
    {
        try
        {
            var transaction =
                await transactionRepository.GetByTransactionIdAsync(request.Result
                    .OriginatorConversationID) as MpesaPaymentTransaction;
            if (transaction == null)
            {
                logger.LogError(
                    $"Mpesa: Transaction not found for OriginatorConversationID: {request.Result.OriginatorConversationID}");
                return Result.Fail(ErrorMessageFormatter.Error("transaction_not_found"));
            }

            if (transaction.MpesaStatus != MpesaPaymentTransactionStatus.IN_PROGRESS)
            {
                logger.LogError($"Mpesa: Transaction {transaction.TransactionId} is not in progress");
                return Result.Fail(ErrorMessageFormatter.Error("transaction_not_in_progress"));
            }

            if (request.Result.ResultCode == (int)ResultCode.Success)
            {
                transaction.Complete();
            }
            else
            {
                string error = string.Empty;
                if (Enum.IsDefined(typeof(ResultCode), request.Result.ResultCode))
                {
                    error = ((ResultCode)request.Result.ResultCode).ToString();
                }
                else
                {
                    error = $"UnknownResultCode - {request.Result.ResultCode} ";
                }

                transaction.SetFailed(error);
                transaction.SetProviderTransactionId(request.Result.TransactionID);
                logger.LogWarning($"Transaction {transaction.TransactionId} failed. ResultCode: {request.Result.ResultCode}, ResultDesc: {request.Result.ResultDesc}");
            }

            await transactionRepository.UpdateAsync(transaction);
            return Result.Ok();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
            return Result.Fail(ErrorMessageFormatter.Error());
        }

    }

    public async Task TransactionStatusQueryAsync(MpesaPaymentTransaction transaction)
    {
        try
        {
            var provider = await paymentProviderRepository.GetAsync(_providerId);
            var requestData = new TransactionStatusQueryRequest()
            {
                Initiator = _mpesaConfig.InitiatorName,
                SecurityCredential = MpesaSecurity.GenerateSecurityCredential(_mpesaConfig.InitiatorPassword, _mpesaConfig.CertificateName),
                CommandID = "TransactionStatusQuery",
                TransactionID = "", // as expected by Mpesa
                OriginalConversationID = transaction.TransactionId,
                PartyA = _mpesaConfig.MerchantId,
                IdentifierType = "4",
                ResultURL = $"{provider.CallbackUrl}/api/payout/statusCheck",
                QueueTimeOutURL = $"{provider.CallbackUrl}/api/payout/statusCheck/timeout",
                Remarks = $"{transaction.TransactionId}_{transaction.AccountNumber}_{transaction.Amount}",
                Occasion = "StatusCheck"
            };

            var responseDoc = await SendMpesaRequestAsync($"{provider.ApiUrl}/mpesa/transactionstatus/v1/query", requestData);

            transaction.SetUnconfirmed(string.Empty);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
            throw;
        }
    }

    public async Task<Result> ProcessPayoutStatusCheckAsync(B2CResultRequestPayout request)
    {
        try
        {
            string originatorConversationId = request.Result.ResultParameters.ResultParameter.GetValue("OriginatorConversationID", "");

            var transaction = await transactionRepository.GetByTransactionIdAsync(originatorConversationId) as MpesaPaymentTransaction;
            if (transaction == null)
            {
                logger.LogError($"Mpesa: Transaction not found for OriginatorConversationID: {originatorConversationId}");
                return Result.Fail(ErrorMessageFormatter.Error("transaction_not_found"));
            }
            if (transaction.MpesaStatus != MpesaPaymentTransactionStatus.UNCONFIRMED)
            {
                logger.LogError($"Mpesa: Transaction {transaction.TransactionId} is not in UNCONFIRMED state");
                return Result.Fail(ErrorMessageFormatter.Error("invalid_transaction_status"));
            }

            Result result = Result.Ok();

            if (request.Result.ResultCode == (int)ResultCode.Success)
            {
                decimal amount = request.Result.ResultParameters.ResultParameter.GetValue("Amount", 0m);

                var validateData = MpesaSecurity.Validate(transaction, amount);
                if (validateData.IsSuccess)
                {
                    transaction.Complete();
                    await transactionRepository.UpdateAsync(transaction);
                    result = Result.Ok();
                }
                else
                {
                    transaction.SetUnconfirmed(validateData.Value);
                    await transactionRepository.UpdateAsync(transaction);
                    logger.LogWarning($"Transaction {transaction.TransactionId} validation failed: {validateData.Value}");
                    result = Result.Fail(ErrorMessageFormatter.Error("transaction_validation_failed"));
                }
            }
            else
            {
                string error = string.Empty;
                if (Enum.IsDefined(typeof(ResultCode), request.Result.ResultCode)) { error = ((ResultCode)request.Result.ResultCode).ToString(); }
                else { error = $"UnknownResultCode - {request.Result.ResultCode} "; }

                transaction.SetFailed(error);
                transaction.SetProviderTransactionId(request.Result.TransactionID);

                result = Result.Fail(ErrorMessageFormatter.Error("status_processing_failed"));
            }

            await transactionRepository.UpdateAsync(transaction);
            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
            return Result.Fail(ErrorMessageFormatter.Error());
        }
    }

    public async Task<Result> ProcessTimeoutRequestAsync(B2CRequest request)
    {
        try
        {
            MpesaPaymentTransaction transaction = await transactionRepository.GetByTransactionIdAsync(request.OriginatorConversationID) as MpesaPaymentTransaction;
            if (transaction is not { MpesaStatus: MpesaPaymentTransactionStatus.PENDING })
            {
                logger.LogError($"Mpesa: Transaction not found for TransID: {request.OriginatorConversationID}");
                return Result.Fail(ErrorMessageFormatter.Error("transaction_not_found"));
            }

            Result result;

            var responseDoc = await SendMpesaRequestAsync($"mpesa/b2c/v3/paymentrequest", request);
            if (responseDoc.IsSuccess)
            {
                if (responseDoc.Value.RootElement.TryGetProperty("ConversationID", out var convId))
                {
                    transaction.SetInProgress(convId.GetString());
                    result = Result.Ok();
                }
                else
                {
                    transaction.SetFailed(null);
                    logger.LogError("ProcessTimeoutRequestAsync failed: Response did not contain ConversationID.");
                    result = Result.Fail(ErrorMessageFormatter.Error("no_conversation_id"));
                }
            }
            else
            {
                transaction.SetFailed(string.Join('|', responseDoc.Errors.ConvertAll(p => p.Message)));
                logger.LogError($"Request failed with error");
                result = responseDoc.ToResult();
            }

            await transactionRepository.UpdateAsync(transaction);
            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
            return Result.Fail(ErrorMessageFormatter.Error());
        }
    }

    public async Task<Result> ProcessStatusCheckTimeoutRequestAsync(TransactionStatusQueryRequest request)
    {
        try
        {
            var transaction = await transactionRepository.GetByTransactionIdAsync(request.OriginalConversationID) as MpesaPaymentTransaction;
            if (transaction is not { MpesaStatus: MpesaPaymentTransactionStatus.IN_PROGRESS })
            {
                logger.LogError("Mpesa: Transaction not found for TransID: {TransactionId}", request.OriginalConversationID);
                return Result.Fail(ErrorMessageFormatter.Error("transaction_not_found"));
            }

            var responseDoc = await SendMpesaRequestAsync($"mpesa/b2c/v3/paymentrequest", request);
            transaction.SetUnconfirmed(null);
            await transactionRepository.UpdateAsync(transaction);
            return Result.Ok();

        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
            return Result.Fail(ErrorMessageFormatter.Error());
        }
    }


    #endregion

    #region Helper Methods

    private async Task<Result<JsonDocument>> SendMpesaRequestAsync(string url, object requestData)
    {
        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new StringContent(JsonSerializer.Serialize(requestData), Encoding.UTF8, "application/json")
        };

        using var response = await _httpClient.SendAsync(httpRequest);
        var content = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            logger.LogError($"HTTP request to {url} failed with status {response.StatusCode} and content: {content}");
            return Result.Fail(content);
            //throw new MpesaRequestException(response.StatusCode, content);
        }

        return Result.Ok(JsonDocument.Parse(content));
    }

    #endregion
}