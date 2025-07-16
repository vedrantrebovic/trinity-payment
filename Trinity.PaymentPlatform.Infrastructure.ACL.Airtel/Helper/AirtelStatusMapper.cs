using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trinity.PaymentPlatform.Infrastructure.ACL.Airtel.Model;

namespace Trinity.PaymentPlatform.Infrastructure.ACL.Airtel.Helper
{
    public static class AirtelStatusMapper
    {
        public static AirtelPayinTransactionStatus? Map(string code)
        {
            return code switch
            {
                "DP00800001000" => AirtelPayinTransactionStatus.Ambiguous,
                "DP00800001001" => AirtelPayinTransactionStatus.Success,
                "DP00800001002" => AirtelPayinTransactionStatus.IncorrectPin,
                "DP00800001003" => AirtelPayinTransactionStatus.LimitExceeded,
                "DP00800001004" => AirtelPayinTransactionStatus.InvalidAmount,
                "DP00800001005" => AirtelPayinTransactionStatus.MissingPin,
                "DP00800001006" => AirtelPayinTransactionStatus.InProcess,
                "DP00800001007" => AirtelPayinTransactionStatus.InsufficientBalance,
                "DP00800001008" => AirtelPayinTransactionStatus.Refused,
                "DP00800001010" => AirtelPayinTransactionStatus.PayeeNotAllowed,
                "DP00800001024" => AirtelPayinTransactionStatus.TimedOut,
                "DP00800001025" => AirtelPayinTransactionStatus.NotFound,
                "DP00800001026" => AirtelPayinTransactionStatus.SignatureMismatch,
                "DP00800001029" => AirtelPayinTransactionStatus.Expired,
                _ => null
            };
        }

        public static AirtelPayoutTransactionStatus? MapPayout(string code)
        {
            return code switch
            {
                "DP00900001000" => AirtelPayoutTransactionStatus.Ambiguous,
                "DP00900001001" => AirtelPayoutTransactionStatus.Success,
                "DP00900001003" => AirtelPayoutTransactionStatus.LimitExceeded,
                "DP00900001004" => AirtelPayoutTransactionStatus.InvalidAmount,
                "DP00900001005" => AirtelPayoutTransactionStatus.Failed,
                "DP00900001006" => AirtelPayoutTransactionStatus.InProgress,
                "DP00900001007" => AirtelPayoutTransactionStatus.InsufficientFunds,
                "DP00900001009" => AirtelPayoutTransactionStatus.InvalidInitiatee,
                "DP00900001010" => AirtelPayoutTransactionStatus.UserNotAllowed,
                "DP00900001011" => AirtelPayoutTransactionStatus.TransactionNotAllowed,
                "DP00900001012" => AirtelPayoutTransactionStatus.InvalidMobileNumber,
                "DP00900001013" => AirtelPayoutTransactionStatus.Refused,
                "DP00900001014" => AirtelPayoutTransactionStatus.TransactionTimedOut,
                "DP00900001015" => AirtelPayoutTransactionStatus.TransactionNotFound,
                "DP00900001016" => AirtelPayoutTransactionStatus.ForbiddenV2,
                "DP00900001017" => AirtelPayoutTransactionStatus.DuplicateTransactionId,
                "DP00900001018" => AirtelPayoutTransactionStatus.Forbidden,
                _ => null
            };
        }
    }
}
