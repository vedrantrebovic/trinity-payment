using System.Text;
using System.Text.Json;

namespace Trinity.PaymentPlatform.Shared;

public class JsonExtensions
{
    public static bool IsJson(string text)
    {
        try
        {
            var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(text).AsSpan());
            reader.Read();
            reader.Skip();
            return true;
        }
        catch (JsonException je)
        {
            return false;
        }
    }
}