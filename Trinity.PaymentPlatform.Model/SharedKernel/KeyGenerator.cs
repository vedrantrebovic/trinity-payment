using System.Security.Cryptography;
using System.Text;

namespace Trinity.PaymentPlatform.Model.SharedKernel;

public static class KeyGenerator
{
    public static string GenerateCode(int size)
    {
        byte[] data = new byte[4*size];
        char[] chars =
            "ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890".ToCharArray();
        char[] numerics = "1234567890".ToCharArray();

        using (var crypto = RandomNumberGenerator.Create())
        {
            crypto.GetBytes(data);
        }
            
        StringBuilder result = new StringBuilder(size);
        for (int i = 0; i < size; i++)
        {
            var rnd = BitConverter.ToUInt32(data, i * 4);
            var idx = rnd % numerics.Length;

            result.Append(numerics[idx]);
        }

        return result.ToString();
    }

    public static string GenerateCode(string prefix, int size)
    {
        int actualSize = size - prefix.Length;
        return $"{prefix}{GenerateCode(actualSize)}";
    }
}