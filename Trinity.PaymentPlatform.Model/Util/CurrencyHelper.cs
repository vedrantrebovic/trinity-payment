using System.Globalization;

namespace Trinity.PaymentPlatform.Model.Util;

public class CurrencyHelper
{
    public static string GetCurrencySymbol(string currencyCode)
    {
        var culture = CultureInfo
            .GetCultures(CultureTypes.SpecificCultures)
            .FirstOrDefault(culture => new RegionInfo(culture.Name).ISOCurrencySymbol == currencyCode);

        if (culture != null) 
            return culture.NumberFormat.CurrencySymbol;

        return currencyCode;
    }
}