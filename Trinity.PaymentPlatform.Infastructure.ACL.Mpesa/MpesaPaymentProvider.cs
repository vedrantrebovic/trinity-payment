using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Trinity.PaymentPlatform.Infastructure.ACL.Mpesa.Models.Mpesa;
using Trinity.PaymentPlatform.Model.Enum;
using Trinity.PaymentPlatform.Model.PaymentTransactionAggregate;
using Trinity.PaymentPlatform.Model.SeedWork;

namespace Trinity.PaymentPlatform.Infastructure.ACL.Mpesa;

public interface IMpesaPaymentProvider
{
    // Token
    Task<string> GetOAuthTokenAsync(string providerUrl, string key, string password);

    // Payin (STK Push)
    Task CreatePayinStkPushAsync(MpesaPayInRequest request); // create transaction 
    Task ConfirmPayinStkPushAsync(PaymentTransaction transaction, Model.PaymentProviderAggregate.PaymentProvider provider, string token); // send to provider payin
    Task ConfirmPayinAsync(MpesaCallbackRequest request); // callback payin 
    Task MPesaExpressQueryAsync(PaymentTransaction transaction, Model.PaymentProviderAggregate.PaymentProvider provider, string token); // status check payin 

    // Payout
    Task ProcessPayoutAsync(MpesaPayoutRequest request); // create transaction
    Task ConfirmPayoutAsync(PaymentTransaction transaction, Model.PaymentProviderAggregate.PaymentProvider provider, string token); // send to provider payou
    Task ProcessB2CResultAsync(B2CResultRequestPayout request); // callback payin
    Task TransactionStatusQueryAsync(PaymentTransaction transaction, Model.PaymentProviderAggregate.PaymentProvider provider, string token); // status check payou
    Task ProcessPayoutStatusCheckAsync(B2CResultRequestPayout request); // callback status check 
    Task ProcessTimeoutRequestAsync(B2CRequest request); // timeout callback payout
    Task ProcessStatusCheckTimeoutRequestAsync(TransactionStatusQueryRequest request); // timeout status check payout 

}

public class MpesaPaymentProvider : IMpesaPaymentProvider
{
    private readonly IPaymentTransactionRepository _transactionRepository;
    private readonly ILogger<MpesaPaymentProvider> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IMemoryCache _memoryCache;
    private readonly IUnitOfWork _unitOfWork;
    private readonly MpesaConfig _mpesaConfig;
    private readonly int _providerId = 1;

    public MpesaPaymentProvider(
        IOptions<MpesaConfig> mpesaConfig,
        IPaymentTransactionRepository transactionRepository,
        ILogger<MpesaPaymentProvider> logger,
        IHttpClientFactory httpClientFactory,
        IMemoryCache memoryCache,
        IUnitOfWork unitOfWork
    )
    {
        _mpesaConfig = mpesaConfig.Value;
        _transactionRepository = transactionRepository;
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _memoryCache = memoryCache;
        _unitOfWork = unitOfWork;
    }

    #region Token

    public async Task<string> GetOAuthTokenAsync(string providerUrl, string key, string password)
    {
        string cacheKey = $"MpesaToken_{providerUrl}_{key}";

        if (_memoryCache.TryGetValue(cacheKey, out string cachedToken))
        {
            return cachedToken;
        }

        var encodedString = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{key}:{password}"));
        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"{providerUrl}/oauth/v1/generate?grant_type=client_credentials");
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", encodedString);

        var client = _httpClientFactory.CreateClient("Mpesa");
        using var response = await client.SendAsync(request);

        if (response.IsSuccessStatusCode)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            var tokenResponse = JsonSerializer.Deserialize<OAuthResponse>(responseContent);

            string token = tokenResponse.AccessToken;

            int expiresInSeconds = Convert.ToInt16(tokenResponse.ExpiresIn);
            var cacheEntryOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(expiresInSeconds - 60)
            };

            _memoryCache.Set(cacheKey, token, cacheEntryOptions);
            return token;
        }
        else
        {
            throw new Exception("Failed to obtain OAuth token");
        }
    }

    #endregion

    #region Payin (STK Push)


    public async Task CreatePayinStkPushAsync(MpesaPayInRequest request)
    {
        try
        {
            await _unitOfWork.BeginTransaction();

            var provider = await _transactionRepository.GetPaymentProviderById(_providerId, _unitOfWork);
            if (provider == null)
            {
                _logger.LogError("Mpesa: Provider {ProviderId} not found", _providerId);
                throw new Exception($"Payment provider with ID {_providerId} not found.");
            }

            var transaction = new PaymentTransaction
            {
                ProviderId = _providerId,
                UserId = request.UserId,
                Amount = request.Amount,
                Status = (int)Status.PENDING,
                Type = (int)TransactionType.PAYIN,
                AccountNumber = request.AccountNumber,
                TransactionId = Guid.NewGuid().ToString()
            };

            await _transactionRepository.CreateTransactionAsync(transaction, _unitOfWork);
            await _unitOfWork.CommitAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    public async Task ConfirmPayinStkPushAsync(PaymentTransaction transaction, DapperRepository.Models.PaymentProvider provider, string token)
    {
        string timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        transaction.Timestamp = timestamp;

        try
        {
            await _unitOfWork.BeginTransactionAsync();

            var requestData = new
            {
                BusinessShortCode = _mpesaConfig.BusinessShortCode,
                Password = MpesaSecurity.GeneratePassword(_mpesaConfig.BusinessShortCode, _mpesaConfig.PayinStkPushKey, timestamp),
                Timestamp = timestamp,
                TransactionType = _mpesaConfig.PayinStkPushTransactionType,
                Amount = transaction.Amount,
                PartyA = transaction.AccountNumber,
                PartyB = _mpesaConfig.BusinessShortCode,
                PhoneNumber = transaction.AccountNumber,
                CallBackURL = $"{provider.CallbackUrl}/api/payin/callback",
                AccountReference = $"PAYIN_{transaction.AccountNumber}", // Account Reference: This is an Alpha-Numeric parameter that is defined by your system as an Identifier of the transaction for the CustomerPayBillOnline transaction type. Along with the business name, this value is also displayed to the customer in the STK Pin Prompt message. Maximum of 12 characters.
                TransactionDesc = "Payin" // This is any additional information / comment that can be sent along with the request from your system.Maximum of 13 Characters.
            };

            var responseDoc = await SendMpesaRequestAsync($"{provider.ApiUrl}/mpesa/stkpush/v1/processrequest", requestData, token);
            using (responseDoc)
            {
                string responseCode = responseDoc.RootElement.GetProperty("ResponseCode").GetString();
                if (responseCode == "0")
                {
                    transaction.MerchantRequestID = responseDoc.RootElement.GetProperty("MerchantRequestID").GetString();
                    transaction.CheckoutRequestID = responseDoc.RootElement.GetProperty("CheckoutRequestID").GetString();
                    transaction.Status = (int)Status.IN_PROGRESS;
                }
                else
                {
                    string resultDesc = responseDoc.RootElement.GetProperty("ResponseDescription").GetString();
                    transaction.Error = resultDesc;
                    transaction.Status = (int)Status.FAILED;
                    _logger.LogError("Mpesa: Transaction {TransactionId} failed with response code {ResponseCode}", transaction.TransactionId, responseCode);
                }
            }

        }
        catch (MpesaRequestException ex)
        {
            transaction.Error = ex.ErrorMessage;
            transaction.Status = (int)Status.FAILED;
            transaction.UpdatedAt = DateTime.UtcNow;
            _logger.LogError($"Request failed with status {ex.StatusCode}. Content: {ex.ResponseContent}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            transaction.Status = (int)Status.UNCONFIRMED;
        }
        finally
        {
            transaction.UpdatedAt = DateTime.UtcNow;
            await _transactionRepository.UpdateTransactionAsync(transaction, _unitOfWork);
            await _unitOfWork.CommitAsync();
        }
    }

    public async Task ConfirmPayinAsync(MpesaCallbackRequest request)
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync();

            var transaction = await _transactionRepository.GetTransactionByMerchantIdAsync(
                request.Body.StkCallback.MerchantRequestID,
                request.Body.StkCallback.CheckoutRequestID,
                _unitOfWork);

            if (transaction == null || transaction.Status != (int)Status.IN_PROGRESS)
            {
                _logger.LogError("Mpesa: Transaction {MerchantRequestID} not found or not in progress", request.Body.StkCallback.MerchantRequestID);
                await _unitOfWork.RollbackAsync();
                return;
            }

            if (request.Body.StkCallback.ResultCode == (int)ResultCode.Success)
            {

                decimal amount = request.Body.StkCallback.CallbackMetadata.Item.GetValue<decimal>("Amount", 0m);

                var validateData = MpesaSecurity.Validate(transaction, amount);
                if (validateData == "OK")
                {
                    transaction.Status = (int)Status.COMPLETED;
                    var wallet = new Wallet { Balance = transaction.Amount, UserId = transaction.UserId };
                    await _transactionRepository.AddCredit(wallet, _unitOfWork);
                }
                else
                {
                    transaction.Status = (int)Status.UNCONFIRMED;
                    transaction.Error = validateData;
                    _logger.LogWarning("Transaction {TransactionId} validation failed: {ValidateData}", transaction.TransactionId, validateData);
                }
            }
            else
            {
                transaction.Status = (int)Status.FAILED;
                transaction.Error = request.Body.StkCallback.ResultDesc;
                _logger.LogWarning("Transaction {TransactionId} failed", transaction.TransactionId);
            }

            transaction.UpdatedAt = DateTime.UtcNow;
            await _transactionRepository.UpdateTransactionAsync(transaction, _unitOfWork);
            await _unitOfWork.CommitAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            await _unitOfWork.RollbackAsync();
        }
    }

    public async Task MPesaExpressQueryAsync(PaymentTransaction transaction, DapperRepository.Models.PaymentProvider provider, string token)
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync();

            var requestData = new
            {
                BusinessShortCode = _mpesaConfig.BusinessShortCode,
                Password = MpesaSecurity.GeneratePassword(_mpesaConfig.BusinessShortCode, _mpesaConfig.PayinStkPushKey, transaction.Timestamp),
                Timestamp = transaction.Timestamp,
                CheckoutRequestID = transaction.CheckoutRequestID
            };

            var responseDoc = await SendMpesaRequestAsync($"{provider.ApiUrl}/mpesa/stkpushquery/v1/query", requestData, token);
            var content = responseDoc.RootElement.GetRawText();
            var expressResponse = JsonSerializer.Deserialize<ExpressResponse>(content);
            if (expressResponse.ResultCode == "0")
            {
                transaction.Status = (int)Status.COMPLETED;
                var wallet = new Wallet { Balance = transaction.Amount, UserId = transaction.UserId };
                await _transactionRepository.AddCredit(wallet, _unitOfWork);
            }
            else
            {
                string resultDesc = responseDoc.RootElement.GetProperty("ResponseDescription").GetString();
                transaction.Error = resultDesc;
                transaction.Status = (int)Status.FAILED;
                _logger.LogError($"Mpesa: Transaction {transaction.TransactionId} failed with response {resultDesc}");
            }
            transaction.UpdatedAt = DateTime.UtcNow;
            await _transactionRepository.UpdateTransactionAsync(transaction, _unitOfWork);
            await _unitOfWork.CommitAsync();
        }
        catch (MpesaRequestException ex)
        {
            transaction.Error = ex.ErrorMessage;
            transaction.Status = (int)Status.FAILED;
            var wallet = new Wallet { Balance = transaction.Amount, UserId = transaction.UserId };
            await _unitOfWork.CommitAsync();
            _logger.LogError($"Request failed with status {ex.StatusCode}. Content: {ex.ResponseContent}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            await _unitOfWork.RollbackAsync();
        }
    }

    #endregion

    #region Payout

    public async Task ProcessPayoutAsync(MpesaPayoutRequest request)
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync();

            var provider = await _transactionRepository.GetPaymentProviderById(_providerId, _unitOfWork);
            if (provider == null)
            {
                _logger.LogError("Mpesa: Provider {ProviderId} not found", _providerId);
                throw new Exception($"Payment provider with ID {_providerId} not found.");
            }

            var transaction = new PaymentTransaction
            {
                UserId = request.UserId,
                Amount = request.Amount,
                Status = (int)Status.PENDING,
                Type = (int)TransactionType.PAYOUT,
                ProviderId = _providerId,
                AccountNumber = request.AccountNumber,
                TransactionId = Guid.NewGuid().ToString()
            };

            await _transactionRepository.CreateTransactionAsync(transaction, _unitOfWork);

            var wallet = new Wallet { Balance = request.Amount, UserId = request.UserId };
            await _transactionRepository.LockBalance(wallet, _unitOfWork);

            await _unitOfWork.CommitAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    public async Task ConfirmPayoutAsync(PaymentTransaction transaction, DapperRepository.Models.PaymentProvider provider, string token)
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync();

            var requestData = new B2CRequest
            {
                OriginatorConversationID = transaction.TransactionId,
                InitiatorName = _mpesaConfig.InitiatorName,
                SecurityCredential = MpesaSecurity.GenerateSecurityCredential(_mpesaConfig.InitiatorPassword, _mpesaConfig.CertificateName),
                CommandID = _mpesaConfig.PayoutCommandID,
                Amount = transaction.Amount,
                PartyA = _mpesaConfig.MerchantId,
                PartyB = transaction.AccountNumber,
                Remarks = $"{transaction.TransactionId}_{transaction.AccountNumber}_{transaction.Amount}",
                QueueTimeOutURL = $"{provider.CallbackUrl}/api/payout/timeout",
                ResultURL = $"{provider.CallbackUrl}/api/payout/result"
            };

            var responseDoc = await SendMpesaRequestAsync($"{provider.ApiUrl}/mpesa/b2c/v3/paymentrequest", requestData, token);
            using (responseDoc)
            {
                if (responseDoc.RootElement.TryGetProperty("ConversationID", out var convId))
                {
                    transaction.Status = (int)Status.IN_PROGRESS;
                    transaction.CheckoutRequestID = convId.GetString();
                }
                else
                {
                    transaction.Status = (int)Status.FAILED;
                    var wallet = new Wallet { Balance = transaction.Amount, UserId = transaction.UserId };
                    await _transactionRepository.UnLockBalance(wallet, _unitOfWork);
                    _logger.LogError("ConfirmPayoutAsync failed: Response did not contain ConversationID.");
                }
            }

        }
        catch (MpesaRequestException ex)
        {
            transaction.Error = ex.ErrorMessage;
            transaction.Status = (int)Status.FAILED;
            var wallet = new Wallet { Balance = transaction.Amount, UserId = transaction.UserId };
            await _transactionRepository.UnLockBalance(wallet, _unitOfWork);
            _logger.LogError($"Request failed with status {ex.StatusCode}. Content: {ex.ResponseContent}");
        }
        catch (Exception ex)
        {
            transaction.Status = (int)Status.UNCONFIRMED;
            _logger.LogError(ex, ex.Message);
        }
        finally
        {
            transaction.UpdatedAt = DateTime.UtcNow;
            await _transactionRepository.UpdateTransactionAsync(transaction, _unitOfWork);
            await _unitOfWork.CommitAsync();
        }
    }

    public async Task ProcessB2CResultAsync(B2CResultRequestPayout request)
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync();

            var transaction = await _transactionRepository.GetTransactionByIdAsync(request.Result.OriginatorConversationID, _unitOfWork);
            if (transaction == null)
            {
                _logger.LogError($"Mpesa: Transaction not found for OriginatorConversationID: {request.Result.OriginatorConversationID}");
                throw new Exception("Transaction not found.");
            }
            if (transaction.Status != (int)Status.IN_PROGRESS)
            {
                _logger.LogError($"Mpesa: Transaction {transaction.TransactionId} is not in progress");
                throw new Exception("Invalid transaction state.");
            }

            if (request.Result.ResultCode == (int)ResultCode.Success)
            {
                transaction.Status = (int)Status.COMPLETED;
                await _transactionRepository.UpdateTransactionAsync(transaction, _unitOfWork);
                var wallet = new Wallet { LockedBalance = transaction.Amount, UserId = transaction.UserId };
                await _transactionRepository.RemoveLockedBalance(wallet, _unitOfWork);
            }
            else
            {
                transaction.Status = (int)Status.FAILED;
                transaction.ProviderTransactionId = request.Result.TransactionID;
                if (Enum.IsDefined(typeof(ResultCode), request.Result.ResultCode)) { transaction.Error = ((ResultCode)request.Result.ResultCode).ToString(); }
                else { transaction.Error = $"UnknownResultCode - {request.Result.ResultCode} "; }
                await _transactionRepository.UpdateTransactionAsync(transaction, _unitOfWork);
                var wallet = new Wallet { Balance = transaction.Amount, UserId = transaction.UserId };
                _logger.LogWarning($"Transaction {transaction.TransactionId} failed. ResultCode: {request.Result.ResultCode}, ResultDesc: {request.Result.ResultDesc}");
                await _transactionRepository.UnLockBalance(wallet, _unitOfWork);
            }
            transaction.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.CommitAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    public async Task TransactionStatusQueryAsync(PaymentTransaction transaction, DapperRepository.Models.PaymentProvider provider, string token)
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync();

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

            var responseDoc = await SendMpesaRequestAsync($"{provider.ApiUrl}/mpesa/transactionstatus/v1/query", requestData, token);

            transaction.Status = (int)Status.UNCONFIRMED;
            transaction.UpdatedAt = DateTime.UtcNow;
            await _transactionRepository.UpdateTransactionAsync(transaction, _unitOfWork);
            await _unitOfWork.CommitAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            await _unitOfWork.RollbackAsync();
        }
    }

    public async Task ProcessPayoutStatusCheckAsync(B2CResultRequestPayout request)
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync();

            string originatorConversationID = request.Result.ResultParameters.ResultParameter.GetValue<string>("OriginatorConversationID", "");

            var transaction = await _transactionRepository.GetTransactionByIdAsync(originatorConversationID, _unitOfWork);
            if (transaction == null)
            {
                _logger.LogError($"Mpesa: Transaction not found for OriginatorConversationID: {originatorConversationID}");
                throw new Exception("Transaction not found.");
            }
            if (transaction.Status != (int)Status.UNCONFIRMED)
            {
                _logger.LogError($"Mpesa: Transaction {transaction.TransactionId} is not in UNCONFIRMED state");
                throw new Exception("Invalid transaction state.");
            }

            if (request.Result.ResultCode == (int)ResultCode.Success)
            {


                decimal amount = request.Result.ResultParameters.ResultParameter.GetValue<decimal>("Amount", 0m);

                var validateData = MpesaSecurity.Validate(transaction, amount);
                if (validateData == "OK")
                {
                    transaction.Status = (int)Status.COMPLETED;
                    transaction.UpdatedAt = DateTime.UtcNow;
                    await _transactionRepository.UpdateTransactionAsync(transaction, _unitOfWork);

                    var wallet = new Wallet { LockedBalance = transaction.Amount, UserId = transaction.UserId };
                    await _transactionRepository.RemoveLockedBalance(wallet, _unitOfWork);
                }
                else
                {
                    transaction.Status = (int)Status.UNCONFIRMED;
                    transaction.Error = validateData;
                    _logger.LogWarning($"Transaction {transaction.TransactionId} validation failed: {validateData}");
                }
            }
            else
            {
                transaction.Status = (int)Status.FAILED;
                if (Enum.IsDefined(typeof(ResultCode), request.Result.ResultCode)) { transaction.Error = ((ResultCode)request.Result.ResultCode).ToString(); }
                else { transaction.Error = $"UnknownResultCode - {request.Result.ResultCode} "; }
                transaction.ProviderTransactionId = request.Result.TransactionID;
                transaction.UpdatedAt = DateTime.UtcNow;
                await _transactionRepository.UpdateTransactionAsync(transaction, _unitOfWork);

                var wallet = new Wallet { Balance = transaction.Amount, UserId = transaction.UserId };
                await _transactionRepository.UnLockBalance(wallet, _unitOfWork);
            }

            await _unitOfWork.CommitAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    public async Task ProcessTimeoutRequestAsync(B2CRequest request)
    {
        PaymentTransaction transaction = null;
        try
        {
            await _unitOfWork.BeginTransactionAsync();

            var provider = await _transactionRepository.GetPaymentProviderById(_providerId, _unitOfWork);
            if (provider == null)
                throw new Exception($"Payment provider with ID {_providerId} not found.");

            transaction = await _transactionRepository.GetTransactionByIdAsync(request.OriginatorConversationID, _unitOfWork);
            if (transaction == null || transaction.Status != (int)Status.PENDING)
            {
                _logger.LogError($"Mpesa: Transaction not found for TransID: {request.OriginatorConversationID}");
                await _unitOfWork.RollbackAsync();
                return;
            }

            var token = await GetOAuthTokenAsync(provider.ApiUrl, provider.CustomerKey, provider.CustomerSecret);

            var responseDoc = await SendMpesaRequestAsync($"{provider.ApiUrl}/mpesa/b2c/v3/paymentrequest", request, token);
            if (responseDoc.RootElement.TryGetProperty("ConversationID", out var convId))
            {
                transaction.Status = (int)Status.IN_PROGRESS;
                transaction.CheckoutRequestID = convId.GetString();
            }
            else
            {
                transaction.Status = (int)Status.FAILED;
                var wallet = new Wallet { Balance = transaction.Amount, UserId = transaction.UserId };
                await _transactionRepository.UnLockBalance(wallet, _unitOfWork);
                _logger.LogError("ProcessTimeoutRequestAsync failed: Response did not contain ConversationID.");
            }

            transaction.UpdatedAt = DateTime.UtcNow;
            await _transactionRepository.UpdateTransactionAsync(transaction, _unitOfWork);
            await _unitOfWork.CommitAsync();
        }
        catch (MpesaRequestException ex)
        {
            transaction.Error = ex.ErrorMessage;
            transaction.Status = (int)Status.FAILED;
            var wallet = new Wallet { Balance = transaction.Amount, UserId = transaction.UserId };
            await _transactionRepository.UnLockBalance(wallet, _unitOfWork);
            await _unitOfWork.CommitAsync();

            _logger.LogError($"Request failed with status {ex.StatusCode}. Content: {ex.ResponseContent}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            await _unitOfWork.RollbackAsync();
        }
    }

    public async Task ProcessStatusCheckTimeoutRequestAsync(TransactionStatusQueryRequest request)
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync();

            var provider = await _transactionRepository.GetPaymentProviderById(_providerId, _unitOfWork);
            if (provider == null)
                throw new Exception($"Payment provider with ID {_providerId} not found.");

            var transaction = await _transactionRepository.GetTransactionByIdAsync(request.OriginalConversationID, _unitOfWork);
            if (transaction == null || transaction.Status != (int)Status.IN_PROGRESS)
            {
                _logger.LogError("Mpesa: Transaction not found for TransID: {TransactionId}", request.OriginalConversationID);
                await _unitOfWork.RollbackAsync();
                return;
            }

            var token = await GetOAuthTokenAsync(provider.ApiUrl, provider.CustomerKey, provider.CustomerSecret);

            var responseDoc = await SendMpesaRequestAsync($"{provider.ApiUrl}/mpesa/b2c/v3/paymentrequest", request, token);

            transaction.Status = (int)Status.UNCONFIRMED;
            transaction.UpdatedAt = DateTime.UtcNow;
            await _transactionRepository.UpdateTransactionAsync(transaction, _unitOfWork);
            await _unitOfWork.CommitAsync();

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            await _unitOfWork.RollbackAsync();
        }
    }


    #endregion

    #region Helper Methods

    private async Task<JsonDocument> SendMpesaRequestAsync(string url, object requestData, string token)
    {
        var client = _httpClientFactory.CreateClient("Mpesa");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new StringContent(JsonSerializer.Serialize(requestData), Encoding.UTF8, "application/json")
        };

        using var response = await client.SendAsync(httpRequest);
        var content = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError($"HTTP request to {url} failed with status {response.StatusCode} and content: {content}");
            throw new MpesaRequestException(response.StatusCode, content);
        }

        return JsonDocument.Parse(content);
    }


    public class MpesaRequestException : Exception
    {
        public HttpStatusCode StatusCode { get; }
        public string ResponseContent { get; }
        public string ErrorMessage { get; }

        public MpesaRequestException(HttpStatusCode statusCode, string responseContent)
            : base($"HTTP Request failed with status {statusCode}")
        {
            StatusCode = statusCode;
            ResponseContent = responseContent;
            ErrorMessage = ExtractErrorMessage(responseContent);
        }

        private static string ExtractErrorMessage(string responseContent)
        {
            try
            {
                using var document = JsonDocument.Parse(responseContent);
                if (document.RootElement.TryGetProperty("errorMessage", out JsonElement errorElement))
                {
                    return errorElement.GetString();
                }
            }
            catch
            {
                return "Unknown error";

            }
            return "Unknown error";
        }
    }


    #endregion


}