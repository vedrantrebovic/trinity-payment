using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Trinity.PaymentPlatform.Infrastructure.ACL.Airtel.Model;

namespace Trinity.PaymentPlatform.Infrastructure.ACL.Airtel.Helper
{
    public class AirtelEncryptionHelper
    {
        public static (string xSignature, string xKey) EncryptPayload(string payloadJson)
        {
            // 1. Generate AES key and IV
            using var aes = Aes.Create();
            aes.KeySize = 256;
            aes.BlockSize = 128;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            aes.GenerateKey();
            aes.GenerateIV();

            var aesKey = aes.Key;
            var aesIv = aes.IV;

            // 2. Encrypt payload using AES key
            var encryptedPayload = EncryptWithAes(payloadJson, aesKey, aesIv);
            var xSignature = Convert.ToBase64String(encryptedPayload);

            // 3. Encode AES key and IV in base64
            var aesKeyBase64 = Convert.ToBase64String(aesKey);
            var aesIvBase64 = Convert.ToBase64String(aesIv);
            var keyIv = $"{aesKeyBase64}:{aesIvBase64}";

            // 4. Encrypt key:iv using RSA public key
            var encryptedKeyIv = EncryptWithRsa(keyIv);
            var xKey = Convert.ToBase64String(encryptedKeyIv);

            return (xSignature, xKey);
        }

        private static byte[] EncryptWithAes(string plainText, byte[] key, byte[] iv)
        {
            using var aes = Aes.Create();
            aes.Key = key;
            aes.IV = iv;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            var plainBytes = Encoding.UTF8.GetBytes(plainText);
            return encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
        }

        private static byte[] EncryptWithRsa(string text)
        {
            string path = Path.Combine(AppContext.BaseDirectory, "airtel_pub.pem");
            string airtelPublicKeyPem = File.ReadAllText(path);

            using var rsa = RSA.Create();
            rsa.ImportFromPem(airtelPublicKeyPem.ToCharArray());

            var bytesToEncrypt = Encoding.UTF8.GetBytes(text);
            return rsa.Encrypt(bytesToEncrypt, RSAEncryptionPadding.OaepSHA256);
        }

        public static bool IsHashValid(AirtelCallbackRequest request, string secretKey)
        {
            string jsonPayload = JsonSerializer.Serialize(request.Transaction);

            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secretKey));
            byte[] hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(jsonPayload));
            string computedHash = Convert.ToBase64String(hashBytes);

            return computedHash == request.Hash;
        }


        public static string EncryptPinV2(string pin)
        {
            string publicKeyPem = @"
-----BEGIN PUBLIC KEY-----
MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEArUj2SQKLCdTqJ3/ZL6nk
h1N3rtjXBBM+0hBUrhJ/VNSMTBixpD+JjeNaHbONcrvJGSstC2tcVfD04s9xGIKr
9TT6hCYaqGojLeuLimVdXzaP5DzDyrHY8mYgHL+/EGRDh+/7B56Gw8UZxOBPtF6W
jjq0TWGcw5YOW1lSPUeaD+kupmDFlMRk26fASELwkYo5NkHgL/w+XzXw8gDZtrNS
6L8UX2mfqdQ9qKpdMP3ztfOUPjmTvIbTKrGLx0U2sUSQINtMxZQzsYaXIGoZ2thv
bIhJMDFBNbznuv1n8b03Q3MAnEK/xCduQBUkUg1syy7jZMT4ETDeFuW2NMZhteaa
dwIDAQAB
-----END PUBLIC KEY-----";

            try
            {
                using RSA rsa = RSA.Create();
                rsa.ImportFromPem(publicKeyPem.ToCharArray());

                if (rsa.KeySize != 2048)
                {
                    throw new Exception($"Key size is {rsa.KeySize}, expected 2048");
                }

                byte[] pinBytes = Encoding.UTF8.GetBytes(pin);

                byte[] encryptedBytes = rsa.Encrypt(
                    pinBytes,
                    RSAEncryptionPadding.OaepSHA256
                );

                return Convert.ToBase64String(encryptedBytes);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public static string EncryptPinV1(string pin)
        {
            const string base64Key = "MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQCkq3XbDI1s8Lu7SpUBP+bqOs/MC6PKWz6n/0UkqTiOZqKqaoZClI3BUDTrSIJsrN1Qx7ivBzsaAYfsB0CygSSWay4iyUcnMVEDrNVOJwtWvHxpyWJC5RfKBrweW9b8klFa/CfKRtkK730apy0Kxjg+7fF0tB4O3Ic9Gxuv4pFkbQIDAQAB"; 

            using var rsa = RSA.Create();
            var keyBytes = Convert.FromBase64String(base64Key);
            rsa.ImportSubjectPublicKeyInfo(keyBytes, out _);

            var encryptedBytes = rsa.Encrypt(
                Encoding.UTF8.GetBytes(pin),
                RSAEncryptionPadding.Pkcs1 
            );

            return Convert.ToBase64String(encryptedBytes);
        }

    }

}
