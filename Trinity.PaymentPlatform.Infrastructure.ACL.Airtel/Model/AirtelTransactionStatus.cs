using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trinity.PaymentPlatform.Infrastructure.ACL.Airtel.Model
{
    public enum AirtelPayinTransactionStatus
    {
        Ambiguous = 1000,                     // DP00800001000
        Success = 1001,                       // DP00800001001
        IncorrectPin = 1002,                  // DP00800001002
        LimitExceeded = 1003,                 // DP00800001003
        InvalidAmount = 1004,                 // DP00800001004
        MissingPin = 1005,                    // DP00800001005
        InProcess = 1006,                     // DP00800001006
        InsufficientBalance = 1007,           // DP00800001007
        Refused = 1008,                       // DP00800001008
        PayeeNotAllowed = 1010,               // DP00800001010
        TimedOut = 1024,                      // DP00800001024
        NotFound = 1025,                      // DP00800001025
        SignatureMismatch = 1026,             // DP00800001026 
        Expired = 1029                        // DP00800001029
    }

    public enum AirtelPayoutTransactionStatus
    {
        Ambiguous = 1000,                     // DP00900001000
        Success = 1001,                       // DP00900001001
        LimitExceeded = 1003,                 // DP00900001003
        InvalidAmount = 1004,                 // DP00900001004
        Failed = 1005,                        // DP00900001005
        InProgress = 1006,                    // DP00900001006
        InsufficientFunds = 1007,             // DP00900001007
        InvalidInitiatee = 1009,              // DP00900001009
        UserNotAllowed = 1010,                // DP00900001010
        TransactionNotAllowed = 1011,         // DP00900001011
        InvalidMobileNumber = 1012,           // DP00900001012
        Refused = 1013,                       // DP00900001013
        TransactionTimedOut = 1014,           // DP00900001014
        TransactionNotFound = 1015,           // DP00900001015
        ForbiddenV2 = 1016,                   // DP00900001016
        DuplicateTransactionId = 1017,        // DP00900001017
        Forbidden = 1018,                     // DP00900001018
    }

}
