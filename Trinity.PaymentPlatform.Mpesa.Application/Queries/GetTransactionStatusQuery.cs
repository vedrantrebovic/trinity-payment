using Dapper;
using Microsoft.Extensions.Logging;
using Npgsql;
using OneOf;
using OneOf.Types;
using Trinity.PaymentPlatform.Application.Shared.Contracts;
using Trinity.PaymentPlatform.Model.Enum;
using Trinity.PaymentPlatform.Model.SharedKernel;
using Trinity.PaymentPlatform.Mpesa.Application.Dto;

namespace Trinity.PaymentPlatform.Mpesa.Application.Queries;

public record GetTransactionStatusQuery(string TransactionReference):IBaseRequest<TransactionStatusDto>;

public class GetTransactionStatusQueryHandler(ILogger<GetTransactionStatusQueryHandler> logger, NpgsqlDataSource dataSource):IBaseRequestHandler<GetTransactionStatusQuery, TransactionStatusDto>
{
    public async Task<OneOf<TransactionStatusDto, None, List<IDomainError>, Exception>> Handle(GetTransactionStatusQuery request, CancellationToken cancellationToken)
    {
        try
        {
            await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
            var reader = await connection.ExecuteReaderAsync(
                "select * from payment_transactions where transaction_id = @tRef",
                new { tRef = request.TransactionReference });

            while (await reader.ReadAsync(cancellationToken))
            {
                long id = reader.GetInt64(reader.GetOrdinal("id"));
                int providerId = reader.GetInt32(reader.GetOrdinal("provider_id"));
                string userId = reader.GetString(reader.GetOrdinal("user_id"));
                int type = reader.GetInt32(reader.GetOrdinal("type"));
                string? providerTransactionId = reader.IsDBNull(reader.GetOrdinal("provider_transaction_id"))?
                    null: reader.GetString(reader.GetOrdinal("provider_transaction_id"));
                string? error = reader.IsDBNull(reader.GetOrdinal("error"))
                    ? null
                    : reader.GetString(reader.GetOrdinal("error"));
                int status = reader.GetInt32(reader.GetOrdinal("status"));

                return new TransactionStatusDto(id, request.TransactionReference, providerId, userId, type,
                    ((TransactionType)type).ToString(), providerTransactionId, error, status,
                    ((PaymentTransactionStatus)status).ToString());
            }

            return new None();
        }
        catch (Exception e)
        {
            logger.LogError(e, e.Message);
            return e;
        }
    }
}