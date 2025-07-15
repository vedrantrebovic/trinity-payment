using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Transactions;
using FluentResults;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Org.BouncyCastle.Asn1.Ocsp;
using Trinity.PaymentPlatform.Infrastructure.ACL.Airtel.Contracts;
using Trinity.PaymentPlatform.Infrastructure.ACL.Airtel.Helper;
using Trinity.PaymentPlatform.Infrastructure.ACL.Airtel.Model;
using Trinity.PaymentPlatform.Model.Enum;
using Trinity.PaymentPlatform.Model.PaymentProviderAggregate;
using Trinity.PaymentPlatform.Model.PaymentTransactionAggregate;
using Trinity.PaymentPlatform.Model.PaymentTransactionOutboxAggregate;
using Trinity.PaymentPlatform.Model.SharedKernel;
using Trinity.PaymentPlatform.Model.Util;

namespace Trinity.PaymentPlatform.Infrastructure.ACL.Airtel.Services;


public class AirtelPaymentProvider(ILogger<AirtelPaymentProvider> logger, IPaymentTransactionRepository transactionRepository, IPaymentProviderRepository paymentProviderRepository,
    IPaymentTransactionOutboxRepository outboxRepository, IOptions<AirtelConfig> airtelConfig, IHttpClientFactory httpClientFactory) : IAirtelPaymentProvider
{
    private readonly int _providerId = 2;
    private readonly AirtelConfig _airtelConfig = airtelConfig.Value;
    private readonly HttpClient _httpClient = httpClientFactory.CreateClient("Airtel");




    public async Task<Result<long>> CreatePayinAsync(AirtelPayInRequest request)
    {
        try
        {
            var existing = await transactionRepository.GetByTransactionIdAsync(request.TransactionReference);
            if (existing != null) return existing.Id;

            var provider = await paymentProviderRepository.GetAsync(_providerId);
            if (provider == null)
            {
                logger.LogError($"Airtel: Provider {_providerId} not found" );
                return ErrorMessageFormatter.FailWithError("payment_provider_not_found");
            }


            var moneyAmount = Money.Create(request.Amount, request.CurrencyCode); 
            if (moneyAmount.IsFailed)
                return moneyAmount.ToResult();

            var transaction = AirtelPaymentTransaction.CreatePayIn(_providerId, request.UserId.ToString(),
                moneyAmount.Value, request.AccountNumber, request.TransactionReference, request.Country);

            await transactionRepository.SaveAsync(transaction);

            return transaction.Id;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
            return ErrorMessageFormatter.FailWithError();
        }
    }

    public async Task ConfirmPayinAsync(AirtelPaymentTransaction transaction)
    {

        try
        {
            var provider = await paymentProviderRepository.GetAsync(transaction.ProviderId);
            if (provider == null)
            {
                logger.LogError("Airtel: Provider not found for ID {ProviderId}", transaction.ProviderId);
                transaction.SetFailed("provider_not_found");
                return;
            }

            var requestData = new
            {
                reference = transaction.AccountNumber,
                subscriber = new
                {
                    country = transaction.Country,
                    currency = transaction.Amount.CurrencyCode,
                    msisdn = transaction.AccountNumber.ToString()
                },
                transaction = new
                {
                    amount = transaction.Amount.Amount,
                    country = transaction.Country,
                    currency = transaction.Amount.CurrencyCode,
                    id = transaction.TransactionId
                }
            };

            string payloadJson = JsonSerializer.Serialize(requestData);

            var (xSignature, xKey) = AirtelEncryptionHelper.EncryptPayload(payloadJson);

            var sendRequestResult = await SendAirtelRequestAsync(
                url: "merchant/v2/payments/",
                requestData: requestData,
                xSignature: xSignature,
                xKey: xKey,
                countryCode:transaction.Country,
                currencyCode:transaction.Amount.CurrencyCode
            );

            if (sendRequestResult.IsFailed)
            {
                transaction.SetFailed(string.Join('|', sendRequestResult.Errors.ConvertAll(e => e.Message)));
                return;
            }

            using var responseDoc = sendRequestResult.Value;
            var status = responseDoc.RootElement.GetProperty("status");

            var responseCode = status.GetProperty("response_code").GetString();



            var transactionStatus = AirtelStatusMapper.Map(responseCode);

            switch (transactionStatus)
            {
                case AirtelPayinTransactionStatus.Success:              
                case AirtelPayinTransactionStatus.Ambiguous:
                case AirtelPayinTransactionStatus.InProcess:
                    transaction.SetInProgress();
                    break;

                case AirtelPayinTransactionStatus.IncorrectPin:
                case AirtelPayinTransactionStatus.LimitExceeded:
                case AirtelPayinTransactionStatus.InvalidAmount:
                case AirtelPayinTransactionStatus.MissingPin:
                case AirtelPayinTransactionStatus.InsufficientBalance:
                case AirtelPayinTransactionStatus.Refused:
                case AirtelPayinTransactionStatus.PayeeNotAllowed:
                case AirtelPayinTransactionStatus.TimedOut:
                case AirtelPayinTransactionStatus.NotFound:
                case AirtelPayinTransactionStatus.SignatureMismatch:
                case AirtelPayinTransactionStatus.Expired:
                    transaction.SetFailed($"{responseCode}: {transactionStatus.Value}");
                    logger.LogWarning($"Airtel: Transaction {transaction.TransactionId} failed with code {responseCode}: {transactionStatus}");
                    break;

                default:
                    transaction.SetUnconfirmed("unknown_status");
                    logger.LogError($"Airtel Enquiry: Unknown response code {responseCode} for Tx { transaction.TransactionId}");
                    break;
            }

        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception during Airtel collection.");
            transaction.SetUnconfirmed("exception");
            throw;
        }
    }


    public async Task<Result> ProcessCallbackPayinAsync(AirtelCallbackRequest request)
    {
        try
        {
            var transaction = await transactionRepository.GetAirtelTransactionIdAsync(
              request.Transaction.Id);

            if (transaction == null || transaction.Status != PaymentTransactionStatus.InProgress)
            {
                logger.LogError("Airtel: Transaction id : {Id} ,AirtelMoneyId : {AirtelMoneyId} not found or not in progress",
                    request.Transaction.Id, request.Transaction.AirtelMoneyId);
                return Result.Fail(ErrorMessageFormatter.Error("transaction_not_found"));
            }

            if (!AirtelEncryptionHelper.IsHashValid(request, _airtelConfig.CallbackSecretKey))
            {
                logger.LogError("Airtel: Hash verification failed for transaction {Id}", request.Transaction.Id);
                return Result.Fail(ErrorMessageFormatter.Error("invalid_hash_signature"));
            }

            transaction.SetAirtelTransactionId(request.Transaction.AirtelMoneyId);


            if (request.Transaction.StatusCode == "TS")
            {
                transaction.Complete();
            }
            else if (request.Transaction.StatusCode == "TF")
            {
                transaction.SetFailed($"Transaction failed on Airtel side : {request.Transaction.Message}");
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

    public async Task ProcessStatusCheckPayinAsync(AirtelPaymentTransaction transaction)
    {
        try
        {
            string url = $"standard/v1/payments/{transaction.TransactionId}";

            var responseResult = await SendAirtelGetRequestAsync(
                url: url,
                countryCode: transaction.Country,
                currencyCode: transaction.Amount.CurrencyCode
            );

            if (responseResult.IsFailed)
            {
                transaction.SetUnconfirmed(string.Join('|', responseResult.Errors.Select(e => e.Message)));
                return;
            }

            using var responseDoc = responseResult.Value;
            var root = responseDoc.RootElement;

            var status = root.GetProperty("status");
            var responseCode = status.GetProperty("response_code").GetString();
            transaction.SetAirtelTransactionId( responseDoc.RootElement.TryGetProperty("data", out var d) &&
                 d.TryGetProperty("transaction", out var t) &&
                 t.TryGetProperty("airtel_money_id", out var a)
                     ? a.GetString()
                     : null
             );

            //responseCode = "DP00800001001";
            var transactionStatus = AirtelStatusMapper.Map(responseCode);

            switch (transactionStatus)
            {

                case AirtelPayinTransactionStatus.Success:
                    var ts = responseDoc.RootElement.GetProperty("data").GetProperty("transaction").GetProperty("status").GetString();
                    if (ts == "TS")
                    {
                        transaction.Complete();
                        break;
                    }
                    else if (ts == "TF")
                    {
                        transaction.SetFailed($"{responseCode}: {transactionStatus}");
                        break;
                    }
                    else
                    {
                        transaction.SetInProgress();
                        break;
                    }

                case AirtelPayinTransactionStatus.Ambiguous:
                case AirtelPayinTransactionStatus.InProcess:
                    transaction.SetInProgress();
                    break;

                case AirtelPayinTransactionStatus.IncorrectPin:
                case AirtelPayinTransactionStatus.LimitExceeded:
                case AirtelPayinTransactionStatus.InvalidAmount:
                case AirtelPayinTransactionStatus.MissingPin:
                case AirtelPayinTransactionStatus.InsufficientBalance:
                case AirtelPayinTransactionStatus.Refused:
                case AirtelPayinTransactionStatus.PayeeNotAllowed:
                case AirtelPayinTransactionStatus.TimedOut:
                case AirtelPayinTransactionStatus.NotFound:
                case AirtelPayinTransactionStatus.SignatureMismatch:
                case AirtelPayinTransactionStatus.Expired:
                    transaction.SetFailed($"{responseCode}: {transactionStatus.Value}");
                    logger.LogWarning("Airtel Enquiry: Transaction {TxId} failed with code {Code}: {Msg}", transaction.TransactionId, responseCode, transactionStatus);
                    break;

                default:
                    transaction.SetUnconfirmed("unknown_status");
                    logger.LogError("Airtel Enquiry: Unknown response code {Code} for Tx {TxId}", responseCode, transaction.TransactionId);
                    break;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception during Airtel status enquiry.");
            transaction.SetUnconfirmed("exception");
            throw;
        }
    }

    public async Task<Result<long>> CreatePayoutAsync(AirtelPayoutRequest request)
    {
        try
        {
            var existing = await transactionRepository.GetByTransactionIdAsync(request.TransactionReference);
            if (existing != null) return existing.Id;

            var provider = await paymentProviderRepository.GetAsync(_providerId);
            if (provider == null)
            {
                logger.LogError($"Airtel: Provider {_providerId} not found");
                return ErrorMessageFormatter.FailWithError("payment_provider_not_found");
            }

            var moneyAmount = Money.Create(request.Amount, request.CurrencyCode); //todo: check if valid currency code

            var transaction = MpesaPaymentTransaction.CreatePayOut(_providerId, request.UserId.ToString(), moneyAmount.Value, request.AccountNumber, request.TransactionReference);

            await transactionRepository.SaveAsync(transaction);

            return transaction.Id;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
            return ErrorMessageFormatter.FailWithError();
        }
    }


    public async Task ConfirmPayoutAsync(AirtelPaymentTransaction transaction)
    {
        try
        {
            var provider = await paymentProviderRepository.GetAsync(transaction.ProviderId);
            if (provider == null)
            {
                logger.LogError("Airtel: Provider not found for ID {ProviderId}", transaction.ProviderId);
                transaction.SetFailed("provider_not_found");
                return;
            }

            var requestData = new
            {
                payee = new
                {
                    currency =transaction.Amount.CurrencyCode,
                    msisdn = transaction.AccountNumber
                },
                reference = transaction.TransactionId,
                pin = AirtelEncryptionHelper.EncryptPinV1(_airtelConfig.DisbursementPin),
                transaction = new
                {
                    amount = transaction.Amount.Amount,
                    id = transaction.TransactionId.ToString(),
                    type = _airtelConfig.TransactionType // "B2B" or "B2C" depending on config
                }
            };

            string payloadJson = JsonSerializer.Serialize(requestData);
            var (xSignature, xKey) = AirtelEncryptionHelper.EncryptPayload(payloadJson);

            var sendRequestResult = await SendAirtelRequestAsync(
                url: "standard/v2/disbursements/",
                requestData: requestData,
                xSignature: xSignature,
                xKey: xKey,
                countryCode: transaction.Country,
                currencyCode: transaction.Amount.CurrencyCode
            );

            if (sendRequestResult.IsFailed)
            {
                transaction.SetFailed(string.Join('|', sendRequestResult.Errors.ConvertAll(e => e.Message)));
                return;
            }

            using var responseDoc = sendRequestResult.Value;
            var status = responseDoc.RootElement.GetProperty("status");
            var responseCode = status.GetProperty("response_code").GetString();
            transaction.SetAirtelTransactionId(
     responseDoc.RootElement.TryGetProperty("data", out var d) &&
     d.TryGetProperty("transaction", out var t) &&
     t.TryGetProperty("airtel_money_id", out var a)
         ? a.GetString()
         : null
 );
            // responseCode = "DP00900001006";

            var transactionStatus = AirtelStatusMapper.MapPayout(responseCode);

            switch (transactionStatus)
            {
                case AirtelPayoutTransactionStatus.Success:
                case AirtelPayoutTransactionStatus.Ambiguous:
                case AirtelPayoutTransactionStatus.InProgress:
                    transaction.SetInProgress();
                    break;

                case AirtelPayoutTransactionStatus.InvalidInitiatee:
                case AirtelPayoutTransactionStatus.DuplicateTransactionId:
                case AirtelPayoutTransactionStatus.TransactionNotFound:
                case AirtelPayoutTransactionStatus.TransactionNotAllowed:
                case AirtelPayoutTransactionStatus.Failed:
                case AirtelPayoutTransactionStatus.Forbidden:
                case AirtelPayoutTransactionStatus.ForbiddenV2:
                case AirtelPayoutTransactionStatus.UserNotAllowed:
                case AirtelPayoutTransactionStatus.TransactionTimedOut:
                case AirtelPayoutTransactionStatus.Refused:
                case AirtelPayoutTransactionStatus.LimitExceeded:
                case AirtelPayoutTransactionStatus.InvalidMobileNumber:
                case AirtelPayoutTransactionStatus.InvalidAmount:
                case AirtelPayoutTransactionStatus.InsufficientFunds:
                    transaction.SetFailed($"{responseCode}: {transactionStatus}");
                    logger.LogWarning($"Airtel: Transaction { transaction.TransactionId} failed with code {responseCode}: {transactionStatus}");
                    break;

                default:
                    transaction.SetUnconfirmed("unknown_status");
                    logger.LogError($"Airtel: Unknown response code {responseCode} for Tx {transaction.TransactionId} , msg {transactionStatus} " );
                    break;
            }
        
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception during Airtel disbursement.");
            transaction.SetUnconfirmed("exception");
            throw;
        }
    }

    public async Task ProcessStatusCheckPayoutAsync(AirtelPaymentTransaction transaction)
    {
        try
        {
            string url = $"/standard/v2/disbursements/{transaction.TransactionId}?transactionType={_airtelConfig.TransactionType}";

            var responseResult = await SendAirtelGetRequestAsync(url, transaction.Country, transaction.Amount.CurrencyCode);
            if (responseResult.IsFailed)
            {
                transaction.SetFailed(string.Join('|', responseResult.Errors.ConvertAll(e => e.Message)));
                return;
            }

            using var doc = responseResult.Value;
            var responseCode = doc.RootElement.GetProperty("status").GetProperty("response_code").GetString();

            var transactionStatus = AirtelStatusMapper.MapPayout(responseCode);
           
            switch (transactionStatus)
            {
                case AirtelPayoutTransactionStatus.Success:            
                    var ts = doc.RootElement.GetProperty("data").GetProperty("transaction").GetProperty("status").GetString();
                    if (ts == "TS"){transaction.Complete();  break;  }
                    else if (ts == "TF") { transaction.SetFailed($"{responseCode}: {transactionStatus}");  break; }
                    else  {   transaction.SetInProgress();break; }

                case AirtelPayoutTransactionStatus.Ambiguous:
                case AirtelPayoutTransactionStatus.InProgress:
                    transaction.SetInProgress();
                    break;

                case AirtelPayoutTransactionStatus.InvalidInitiatee:
                case AirtelPayoutTransactionStatus.DuplicateTransactionId:
                case AirtelPayoutTransactionStatus.TransactionNotFound:
                case AirtelPayoutTransactionStatus.TransactionNotAllowed:
                case AirtelPayoutTransactionStatus.Failed:
                case AirtelPayoutTransactionStatus.Forbidden:
                case AirtelPayoutTransactionStatus.ForbiddenV2:
                case AirtelPayoutTransactionStatus.UserNotAllowed:
                case AirtelPayoutTransactionStatus.TransactionTimedOut:
                case AirtelPayoutTransactionStatus.Refused:
                case AirtelPayoutTransactionStatus.LimitExceeded:
                case AirtelPayoutTransactionStatus.InvalidMobileNumber:
                case AirtelPayoutTransactionStatus.InvalidAmount:
                case AirtelPayoutTransactionStatus.InsufficientFunds:
                    transaction.SetFailed($"{responseCode}: {transactionStatus}");
                    break;

                default:
                    transaction.SetUnconfirmed("unknown_status");
                    logger.LogError($"Airtel: Unknown status check code {responseCode} for Tx {transaction.TransactionId}" );
                    break;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
            transaction.SetUnconfirmed("exception");
            throw;
        }
    }

    public async Task<Result> ProcessRefundAsync(AirtelRefundRequest request)
    {

        try
        {

            var transaction = await transactionRepository.GetAirtelTransactionIdAsync(request.TransactionReference);

            if (transaction == null || transaction.Status != PaymentTransactionStatus.Completed)
            {
                logger.LogError($"Airtel: Transaction id : {transaction.Id} ,AirtelMoneyId : {transaction.AirtelTransactionId} not found or not in progress");
                return Result.Fail(ErrorMessageFormatter.Error("transaction_not_found"));
            }


            var requestData = new
            {

                transaction = new
                {
                    airtel_money_id = transaction.AirtelTransactionId

                }
            };

            string payloadJson = JsonSerializer.Serialize(requestData);
            var (xSignature, xKey) = AirtelEncryptionHelper.EncryptPayload(payloadJson);

            var sendRequestResult = await SendAirtelRequestAsync(
                url: "standard/v2/payments/refund",
                requestData: requestData,
                xSignature: xSignature,
                xKey: xKey,
                countryCode: transaction.Country,
                currencyCode: transaction.Amount.CurrencyCode
            );


            using var responseDoc = sendRequestResult.Value;
            var status = responseDoc.RootElement.GetProperty("status");
            var responseCode = status.GetProperty("response_code").GetString();

            var transactionStatus = AirtelStatusMapper.Map(responseCode);

            switch (transactionStatus)
            {

                case AirtelPayinTransactionStatus.Success:
                    transaction.Refund();
                    break;
                default:
                    logger.LogError($"Airtel refund failed: Response code {responseCode} for Tx {transaction.TransactionId}");
                    break;
            }
          
            return Result.Ok();

        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception during Airtel refund.");
            throw;
        }
    }
    




    #region Helper Methods

    private async Task<Result<JsonDocument>> SendAirtelRequestAsync(
      string url,
      object requestData,
      string xSignature,
      string xKey,
      string countryCode,
      string currencyCode)
    {
        var json = JsonSerializer.Serialize(requestData);
        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };

        httpRequest.Headers.Add("X-Country", countryCode);
        httpRequest.Headers.Add("X-Currency", currencyCode);
        if (!string.IsNullOrEmpty(xSignature))
        {
            httpRequest.Headers.Add("x-signature", xSignature);
        }
        
        if (!string.IsNullOrEmpty(xKey))
        {
            httpRequest.Headers.Add("x-key", xKey);
        }

        httpRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));

        using var response = await _httpClient.SendAsync(httpRequest);
        var content = await response.Content.ReadAsStringAsync();

       // content = "\r\n{\r\n    \"data\": {\r\n        \"transaction\": {\r\n            \"reference_id\": \"CI250704.1516.A00607\",\r\n            \"airtel_money_id\": \"disbursement-KU7ZN2X5F9-gddjdadfsdffsfdssasf\",\r\n            \"id\": \"gddjdadfsdffsfdssasf\"\r\n        }\r\n    },\r\n    \"status\": {\r\n        \"response_code\": \"DP00900001001\",\r\n        \"code\": \"200\",\r\n        \"success\": true,\r\n        \"result_code\": \"ESB000010\",\r\n        \"message\": \"*151*Txn ID CI250704.1516.A00607*S* Cashin of RWF100 to 733400208 Belyse HAKIZIMANA UWIHIRWE Successful. Comission 0 BALANCE RWF1355200. *EN#\"\r\n    }\r\n}";
        if (!response.IsSuccessStatusCode)
        {
            logger.LogError($"HTTP request to {url} failed with status {response.StatusCode} and content: {content}");
            return Result.Fail(content);
        }

        return Result.Ok(JsonDocument.Parse(content));
    }

    private async Task<Result<JsonDocument>> SendAirtelGetRequestAsync(
    string url,
    string countryCode,
    string currencyCode)
    {
        using var httpRequest = new HttpRequestMessage(HttpMethod.Get, url);
        httpRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));
        httpRequest.Headers.Add("X-Country", countryCode);
        httpRequest.Headers.Add("X-Currency", currencyCode);

        using var response = await _httpClient.SendAsync(httpRequest);
        var content = await response.Content.ReadAsStringAsync();
       // content = "{\r\n    \"data\": {\r\n        \"transaction\": {\r\n            \"airtel_money_id\": \"C36*******67\",\r\n            \"id\": \"payin20\",\r\n            \"message\": \"success\",\r\n            \"status\": \"TS\"\r\n        }\r\n    },\r\n    \"status\": {\r\n        \"code\": \"200\",\r\n        \"message\": \"SUCCESS\",\r\n        \"result_code\": \"ESB000010\",\r\n        \"response_code\": \"DP00800001001\",\r\n        \"success\": false\r\n    }\r\n}";
        if (!response.IsSuccessStatusCode)
        {
            logger.LogError($"Airtel GET request to {url} failed with status {response.StatusCode} and content: {content}");
            return Result.Fail(content);
        }

        return Result.Ok(JsonDocument.Parse(content));
    }

  


    #endregion

}
