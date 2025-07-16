using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using FluentResults;
using Trinity.PaymentPlatform.Model.PaymentTransactionAggregate;
using Trinity.PaymentPlatform.Model.Util;

namespace Trinity.PaymentPlatform.Infrastructure.ACL.Mpesa;

public static class MpesaSecurity
{
  
    public static string GenerateSecurityCredential(string initiatorPassword, string certificateSubjectName)
    {
        X509Certificate2? cert = GetCertificateFromLocalMachine(certificateSubjectName);
        if(cert==null)
            throw new Exception($"Certificate with subject name '{certificateSubjectName}' was not found in the Local Machine store.");

        using RSA publicKey = cert.GetRSAPublicKey();
        byte[] encryptedBytes = publicKey.Encrypt(
            Encoding.UTF8.GetBytes(initiatorPassword),
            RSAEncryptionPadding.Pkcs1);

        return Convert.ToBase64String(encryptedBytes);
    }

    private static X509Certificate2? GetCertificateFromLocalMachine(string subjectName)
    {

        foreach (StoreName storeName in (StoreName[])
                 Enum.GetValues(typeof(StoreName)))
        {
            using (X509Store store = new X509Store(storeName, StoreLocation.LocalMachine))
            {
                store.Open(OpenFlags.ReadOnly);

                X509Certificate2Collection certCollection = store.Certificates.Find(
                    X509FindType.FindBySubjectName, subjectName, validOnly: false);

                if (certCollection.Count > 0)
                {
                    return certCollection[0];
                }
            }
        }

        return null;
    }


    public static string GeneratePassword(string businessShortCode, string passkey, string timestamp)
    {
        var rawPassword = businessShortCode + passkey + timestamp;
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(rawPassword));
    }


    public static Result<string> Validate(MpesaPaymentTransaction transaction, decimal amount, string? accountNumber = null,  string? timestamp = null )
    {
        if (transaction.Amount.Amount != amount)
        {
            return ErrorMessageFormatter.FailWithError("invalid_amount");
        }
        if (!string.IsNullOrEmpty(accountNumber) && accountNumber != transaction.AccountNumber)
        {
            return ErrorMessageFormatter.FailWithError("invalid_account_number");
        }
        if (!string.IsNullOrEmpty(timestamp) && timestamp != transaction.ProviderTimestamp)
        {
            return ErrorMessageFormatter.FailWithError("invalid_timestamp");
        }
        return Result.Ok();
    }
}