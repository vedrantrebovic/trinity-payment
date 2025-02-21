namespace Trinity.PaymentPlatform.Model.Util;

public static class DictionaryExtensions
{
    private static readonly char SpecifiersSeparator = '|';
    public static string? FormatSpecifiers(this IReadOnlyDictionary<string, string>? specifiers)
    {
        if (specifiers == null || specifiers.Count == 0) return null;
        return string.Join(SpecifiersSeparator, specifiers.OrderBy(p => p.Key).Select(x => $"{x.Key}={x.Value}"));
    }

    public static string? FormatSpecifiers(this IDictionary<string, string>? specifiers)
    {
        if (specifiers == null || specifiers.Count == 0) return null;
        return string.Join(SpecifiersSeparator, specifiers.OrderBy(p => p.Key).Select(x => $"{x.Key}={x.Value}"));
    }

    public static Dictionary<string, string> ToDictionary(this string value)
    {
        return string.IsNullOrEmpty(value)
            ? new Dictionary<string, string>()
            : value.Split(SpecifiersSeparator).GroupBy(p => p.Split('=')[0], x => x.Split('=')[1])
                .ToDictionary(g => g.Key, g => g.First());
    }
}