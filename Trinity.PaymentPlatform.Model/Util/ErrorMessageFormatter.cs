using FluentResults;

namespace Trinity.PaymentPlatform.Model.Util;

public static class ErrorMessageFormatter
{
    private const char Separator = ':';
    private const string ErrorPrefix = "err";
    private const string WarningPrefix = "wrn";
    private const string MessagePrefix = "msg";
    private const string InformationPrefix = "inf";
    private const string LabelPrefix = "lbl";
    private const string ButtonPrefix = "btn";

    public static string Error(string key = "exception") => string.Join(Separator, ErrorPrefix, key);
    public static Result FailWithError(string key = "exception") => Fail(key);
    public static string Warning(string key) => string.Join(Separator, WarningPrefix, key);
    public static Result FailWithWarning(string key = "warning") => Fail(string.Join(Separator, WarningPrefix, key));
    public static string Message(string key) => string.Join(Separator, MessagePrefix, key);
    public static string Information(string key) => string.Join(Separator, InformationPrefix, key);
    public static string Label(string key) => string.Join(Separator, LabelPrefix, key);
    public static string Button(string key) => string.Join(Separator, ButtonPrefix, key);

    private static Result Fail(string errorMessage)
    {
        return Result.Fail(Error(errorMessage));
    }
}