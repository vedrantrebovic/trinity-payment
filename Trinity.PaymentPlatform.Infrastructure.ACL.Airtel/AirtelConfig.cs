using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trinity.PaymentPlatform.Infrastructure.ACL.Airtel
{
    public class AirtelConfig
    {
        public static string SectionName => "AirtelConfig";

        public string MerchantId { get; set; }
        public string ApiUrl { get; set; }
        public string ClientId { get; set; }
        public string CustomerSecret { get; set; }
        public string Country { get; set; }
        public string Currency { get; set; }
        public string CallbackSecretKey { get; set; }
        public string DisbursementPin  { get; set; }
        public string TransactionType { get; set; }
        public int StatusCheckDelay { get; set; }
        public int StatusCheckLimit { get; set; }

    }
}
