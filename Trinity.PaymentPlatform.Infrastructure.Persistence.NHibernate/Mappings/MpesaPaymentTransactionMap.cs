using FluentNHibernate.Mapping;
using Trinity.PaymentPlatform.Model.Enum;
using Trinity.PaymentPlatform.Model.PaymentTransactionAggregate;

namespace Trinity.PaymentPlatform.Infrastructure.Persistence.NHibernate.Mappings;

public class MpesaPaymentTransactionMap: SubclassMap<MpesaPaymentTransaction>
{
    public MpesaPaymentTransactionMap()
    {
        Table("payment_transactions_mpesa");

        Map(x => x.MpesaStatus, "mpesa_status").CustomType<MpesaPaymentTransactionStatus>();
        Map(x => x.AccountNumber, "account_number");
        Map(x => x.MerchantRequestId, "merchant_request_id");
        Map(x => x.CheckoutRequestId, "checkout_request_id");
        Map(x => x.ProviderTimestamp, "provider_timestamp");
    }
}