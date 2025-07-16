using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trinity.PaymentPlatform.Infrastructure.ACL.Airtel.Model
{
    public record AirtelPayoutRequest(
       string UserId,
       decimal Amount,
       string CurrencyCode,
       string AccountNumber,
       string TransactionReference);
}
