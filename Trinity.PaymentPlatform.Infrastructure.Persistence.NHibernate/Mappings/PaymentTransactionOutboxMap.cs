using FluentNHibernate.Mapping;
using Trinity.PaymentPlatform.Model.Enum;
using Trinity.PaymentPlatform.Model.PaymentTransactionOutboxAggregate;
using Trinity.PaymentPlatform.Model.SharedKernel;

namespace Trinity.PaymentPlatform.Infrastructure.Persistence.NHibernate.Mappings;

public class PaymentTransactionOutboxMap:ClassMap<PaymentTransactionOutbox>
{
    public PaymentTransactionOutboxMap()
    {
        Table("payment_transactions_outbox");
        Id(x => x.Id, "id").GeneratedBy.Sequence("payment_transactions_outbox_id_seq");

        #region Auditing

        Map(x => x.CreatedAt, "created_at");
        Map(x => x.ModifiedAt, "modified_at");

        #endregion

        Map(x => x.TransactionReference, "transaction_reference").Unique();
        Map(x => x.ProviderTransactionId, "provider_transaction_id");
        Map(x => x.TransactionId, "transaction_id");
        Map(x => x.CallbackUrl, "callback_url");
        Map(x => x.Sent, "sent");
        Map(x => x.Success, "success");
        Map(x => x.Error, "error");
        Map(x => x.ResendCount, "resend_count");
        Map(x => x.BackOffUntil, "back_off_until").CustomSqlType("timestamp with time zone");
        Map(x => x.Type, "type").CustomType<TransactionType>();

        Component<Money>(x => x.Amount, p =>
        {
            p.Map(x => x.Amount, "amount");
            p.Map(x => x.CurrencyCode, "currency_code");
        });
    }
}