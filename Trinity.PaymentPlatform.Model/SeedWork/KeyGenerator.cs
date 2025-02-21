using Trinity.PaymentPlatform.Model.Util;

namespace Trinity.PaymentPlatform.Model.SeedWork;

public static class KeyGenerator
{
    private const string PaymentPrefix = "PAY";
    private const string RefundPrefix = "PAYR";
    private const string GrantPrefix = "GRT";
    private const string CustomerPrefix = "CST";
    private const string MerchantPrefix = "MCT";
    private const string DirectDepositPrefix = "DDT";
    private const string DirectWithdrawalPrefix = "DWW";
    private const string PaymentMethodPrefix = "PMT";
    private const string DirectMoneyTransferPrefix = "DMT";

    public static string GeneratePaymentKey()
    {
        return $"{PaymentPrefix}_{RandomKeyGenerator.GetUniqueAlphaNumericKey(25)}";
    }

    public static string GenerateRefundKey()
    {
        return $"{RefundPrefix}_{RandomKeyGenerator.GetUniqueAlphaNumericKey(25)}";
    }

    public static string GenerateGrantKey()
    {
        return $"{GrantPrefix}_{RandomKeyGenerator.GetUniqueAlphaNumericKey(128)}";
    }

    public static string GenerateCustomerKey()
    {
        return $"{CustomerPrefix}_{RandomKeyGenerator.GetUniqueAlphaNumericKey(50)}";
    }

    public static string GenerateMerchantKey()
    {
        return $"{MerchantPrefix}_{RandomKeyGenerator.GetUniqueAlphaNumericKey(25)}";
    }

    public static string GenerateDirectDepositKey()
    {
        return $"{DirectDepositPrefix}_{RandomKeyGenerator.GetUniqueAlphaNumericKey(25)}";
    }

    public static string GenerateDirectWithdrawalKey()
    {
        return $"{DirectWithdrawalPrefix}_{RandomKeyGenerator.GetUniqueAlphaNumericKey(25)}";
    }

    public static string GeneratePaymentMethodKey()
    {
        return $"{PaymentMethodPrefix}_{RandomKeyGenerator.GetUniqueAlphaNumericKey(25)}";
    }

    public static string GenerateDirectMoneyTransferKey()
    {
        return $"{DirectMoneyTransferPrefix}_{RandomKeyGenerator.GetUniqueAlphaNumericKey(25)}";
    }
}