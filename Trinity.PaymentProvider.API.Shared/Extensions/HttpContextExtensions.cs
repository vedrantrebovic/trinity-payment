using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using TimeZoneConverter;

namespace Trinity.PaymentProvider.API.Shared.Extensions;

public static class HttpContextExtensions
{
    public static string GetIpAddress(this HttpContext context)
    {
        string ip = "";

        // todo support new "Forwarded" header (2014) https://en.wikipedia.org/wiki/X-Forwarded-For

        // X-Forwarded-For (csv list):  Using the First entry in the list seems to work
        // for 99% of cases however it has been suggested that a better (although tedious)
        // approach might be to read each IP from right to left and use the first public IP.
        // http://stackoverflow.com/a/43554000/538763
        //
        ip = GetHeaderValueAs<string>("X-Forwarded-For", context).SplitCsv().FirstOrDefault();

        // RemoteIpAddress is always null in DNX RC1 Update1 (bug).
        if (String.IsNullOrWhiteSpace(ip) && context?.Connection?.RemoteIpAddress != null)
            ip = context.Connection.RemoteIpAddress.ToString();

        if (String.IsNullOrWhiteSpace(ip))
            ip = GetHeaderValueAs<string>("REMOTE_ADDR", context);

        // _httpContextAccessor.HttpContext?.Request?.Host this is the local host.

        return ip;
    }

    public static uint? GetIpAddressNumeric(this HttpContext context)
    {
        string ip = context.GetIpAddress();
        if (!string.IsNullOrEmpty(ip))
        {
            try
            {
                if (IPAddress.TryParse(ip, out var address))
                {
                    var bytes = address.GetAddressBytes();
                    if(BitConverter.IsLittleEndian)
                        Array.Reverse(bytes);

                    return BitConverter.ToUInt32(bytes);
                }

                return null;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        return null;
    }
    
    private static T GetHeaderValueAs<T>(string headerName, HttpContext context)
    {
        if (context?.Request?.Headers?.TryGetValue(headerName, out var values) ?? false)
        {
            string rawValues = values.ToString();   // writes out as Csv when there are multiple.

            if (!String.IsNullOrWhiteSpace(rawValues))
                return (T)Convert.ChangeType(values.ToString(), typeof(T));
        }
        return default(T);
    }
    
    private static List<string> SplitCsv(this string csvList, bool nullOrWhitespaceInputReturnsNull = false)
    {
        if (string.IsNullOrWhiteSpace(csvList))
            return nullOrWhitespaceInputReturnsNull ? null : new List<string>();

        return csvList
            .TrimEnd(',')
            .Split(',')
            .AsEnumerable<string>()
            .Select(s => s.Trim())
            .ToList();
    }

    public static string GetLanguage(this HttpContext context)
    {
        string langId = GetHeaderValueAs<string>(HeaderNames.AcceptLanguage, context);
        if (string.IsNullOrEmpty(langId)) langId = "en";
        return langId;
    }
    
    public static Guid GetAppId(this HttpContext context)
    {
        string appIdString = GetHeaderValueAs<string>("Application-Id", context);
        if (Guid.TryParse(appIdString, out Guid appId)) return appId;
        return default;
    }

    public static string GetTimezoneString(this HttpContext context)
    {
        string tz = GetHeaderValueAs<string>("Accept-Timezone", context);
        if (string.IsNullOrEmpty(tz)) tz = "Europe/Brussels";

        return tz;
    }

    public static TimeZoneInfo GetTimezone(this HttpContext context)
    {
        if (TZConvert.TryGetTimeZoneInfo(context.GetTimezoneString(), out var tzInfo)) return tzInfo;
        return TZConvert.GetTimeZoneInfo("Europe/Brussels");
    }

    public static string GetUserAgent(this HttpContext context) => context.Request.Headers["User-Agent"].ToString();
}