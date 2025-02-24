namespace Trinity.PaymentProvider.API.Shared.Extensions;

public static class DateTimeExtensions
{
    public const int DefaultDays = 90;
    public const int DefaultHours = DefaultDays * 24;
    public const int DefaultMinutes = DefaultHours * 60;
    public static DateTime GetTimeFromDaysInTimezone(int? days, TimeZoneInfo tz)
    {
        return DateTime.UtcNow.AddDays(days ?? DefaultDays).Date.Subtract(tz.BaseUtcOffset);
    }
    
    public static DateTime GetTimeFromHoursInTimezone(int? hours, TimeZoneInfo tz)
    {
        return DateTime.UtcNow.AddHours(hours ?? DefaultHours).Subtract(tz.BaseUtcOffset);
    }
    
    public static DateTime GetTimeFromMinutesInTimezone(int? minutes, TimeZoneInfo tz)
    {
        return DateTime.UtcNow.AddMinutes(minutes ?? DefaultMinutes).Subtract(tz.BaseUtcOffset);
    }
}