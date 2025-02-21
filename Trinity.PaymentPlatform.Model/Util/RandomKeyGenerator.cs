using System.Security.Cryptography;
using System.Text;

//using HashidsNet;

namespace Trinity.PaymentPlatform.Model.Util;

public static class RandomKeyGenerator
{
    private static readonly char[] alphaNumeric = 
        "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890".ToCharArray();

    private static readonly char[] numeric = "1234567890".ToCharArray();
    //private static readonly Hashids Hashids = new Hashids("x6BLmzP3qpd5v0OCNcXAYSyd3swT8c");

    public static string GetUniqueNumericKey(int length)
    {
        byte[] data = new byte[length * 4];
        data = RandomNumberGenerator.GetBytes(data.Length);
        
        StringBuilder result = new StringBuilder(length);
        for (int i = 0; i < length; i++)
        {
            var rnd = BitConverter.ToUInt32(data, i * 4);
            var idx = rnd % numeric.Length;

            result.Append(numeric[idx]);
        }

        return result.ToString();
    }
    
    public static string GetUniqueAlphaNumericKey(int length)
    {
        byte[] data = new byte[length * 4];
        data = RandomNumberGenerator.GetBytes(data.Length);
        
        StringBuilder result = new StringBuilder(length);
        for (int i = 0; i < length; i++)
        {
            var rnd = BitConverter.ToUInt32(data, i * 4);
            var idx = rnd % alphaNumeric.Length;

            result.Append(alphaNumeric[idx]);
        }

        return result.ToString();
    }

    // public static string HashId(IList<long> ids)
    // {
    //     return Hashids.EncodeLong(ids);
    // }
}