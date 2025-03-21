﻿namespace Trinity.PaymentPlatform.Infrastructure.ACL.Mpesa
{
    public class MpesaConfig
    {
        public static string SectionName => "MpesaConfig";

        public string MerchantId { get; set; }
        public string InitiatorName { get; set; }
        public string CertificateName { get; set; }
        public string PayoutCommandID { get; set; }
        public string InitiatorPassword { get; set; }
        public string PayinStkPushTransactionType { get; set; } 
        public string BusinessShortCode { get; set; }
        public string PayinStkPushKey { get; set; }
        public string ApiUrl { get; set; }
        public string CustomerKey { get; set; }
        public string CustomerSecret { get; set; }
    }
}
