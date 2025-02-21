using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Trinity.PaymentPlatform.Infastructure.ACL.Mpesa;

public static class MpesaSecurity
{
  
    public static string GenerateSecurityCredential(string initiatorPassword, string certificateSubjectName)
    {
        X509Certificate2 cert = GetCertificateFromLocalMachine(certificateSubjectName);

        using (RSA publicKey = cert.GetRSAPublicKey())
        {
           
            byte[] encryptedBytes = publicKey.Encrypt(
                Encoding.UTF8.GetBytes(initiatorPassword),
                RSAEncryptionPadding.Pkcs1);

            return Convert.ToBase64String(encryptedBytes);
        }
    }

    private static X509Certificate2 GetCertificateFromLocalMachine(string subjectName)
    {
        using (X509Store store = new X509Store(StoreName.My, StoreLocation.LocalMachine))
        {
            store.Open(OpenFlags.ReadOnly);

     
            X509Certificate2Collection certCollection = store.Certificates.Find(
                X509FindType.FindBySubjectName, subjectName, validOnly: false);

            if (certCollection.Count == 0)
            {
                throw new Exception($"Certificate with subject name '{subjectName}' was not found in the Local Machine store.");
            }

            return certCollection[0];
        }
    }


    public static string GeneratePassword(string businessShortCode, string passkey, string timestamp)
    {
        var rawPassword = businessShortCode + passkey + timestamp;
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(rawPassword));
    }


    public static string Validate(PaymentTransaction transaction, decimal amount, string accountNumber = null,  string timestamp = null )
    {
        if (transaction.Amount != amount)
        {
            return "Invalid Amount";
        }
        if (!string.IsNullOrEmpty(accountNumber) && accountNumber != transaction.AccountNumber)
        {
            return "Invalid Account Number";
        }
        if (!string.IsNullOrEmpty(timestamp)  && timestamp != transaction.Timestamp)
        {
            return "Invalid Timestamp";
        }
        return "OK";
    }

}