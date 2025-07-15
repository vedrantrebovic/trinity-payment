using FluentNHibernate.Mapping;
using Trinity.PaymentPlatform.Model.Enum;
using Trinity.PaymentPlatform.Model.PaymentTransactionAggregate;

namespace Trinity.PaymentPlatform.Infrastructure.Persistence.NHibernate.Mappings;

public class AirtelPaymentTransactionMap: SubclassMap<AirtelPaymentTransaction>
{
    public AirtelPaymentTransactionMap()
    {
        Table("payment_transactions_airtel");

        Map(x => x.AirtelStatus, "airtel_status").CustomType<AirtelPaymentTransactionStatus>();
        Map(x => x.AccountNumber, "account_number");
        Map(x => x.Country, "country");
        Map(x => x.AirtelTransactionId, "airtel_transaction_id");


    }
}