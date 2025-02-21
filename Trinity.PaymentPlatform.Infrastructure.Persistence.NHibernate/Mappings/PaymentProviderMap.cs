using FluentNHibernate.Mapping;
using Trinity.PaymentPlatform.Model.PaymentProviderAggregate;

namespace Trinity.PaymentPlatform.Infrastructure.Persistence.NHibernate.Mappings;

public class PaymentProviderMap:ClassMap<PaymentProvider>
{
    public PaymentProviderMap()
    {
        Table("payment_providers");
        Id(x => x.Id, "id").GeneratedBy.Sequence("payment_providers_id_seq");

        #region Auditing

        Map(x => x.CreatedAt, "created_at");
        Map(x => x.ModifiedAt, "modified_at");

        #endregion

        Map(x => x.Name, "name");
        Map(x => x.ApiUrl, "api_url");
        Map(x => x.IsActive, "is_active");
        Map(x => x.MerchantId, "merchant_id");
        Map(x => x.CustomerKey, "customer_key");
        Map(x => x.CustomerSecret, "customer_secret");
        Map(x => x.CallbackUrl, "callback_url");
    }
}