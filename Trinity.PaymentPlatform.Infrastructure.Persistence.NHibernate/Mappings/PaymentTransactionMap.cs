using FluentNHibernate.Mapping;
using Trinity.PaymentPlatform.Model.Enum;
using Trinity.PaymentPlatform.Model.PaymentTransactionAggregate;
using Trinity.PaymentPlatform.Model.SharedKernel;

namespace Trinity.PaymentPlatform.Infrastructure.Persistence.NHibernate.Mappings;

public class PaymentTransactionMap:ClassMap<PaymentTransaction>
{
    public PaymentTransactionMap()
    {
        Table("payment_transactions");
        Id(x => x.Id, "id").GeneratedBy.Sequence("payment_transactions_id_seq");

        #region Auditing

        Map(x => x.CreatedAt, "created_at");
        Map(x => x.ModifiedAt, "modified_at");

        #endregion

        Map(x => x.ProviderId, "provider_id");
        Map(x => x.UserId, "user_id");
        Map(x => x.Type, "type").CustomType<TransactionType>();
        Map(x => x.TransactionId, "transaction_id");
        Map(x => x.ProviderTransactionId, "provider_transaction_id");
        Map(x => x.Error, "error");
        Map(x => x.StatusFinalizationTime, "status_finalization_time").CustomSqlType("timestamp with time zone");
        Map(x => x.Status, "status").CustomType<PaymentTransactionStatus>();

        Component<Money>(x => x.Amount, p =>
        {
            p.Map(x => x.Amount, "amount");
            p.Map(x => x.CurrencyCode, "currency_code");
        });
    }
}