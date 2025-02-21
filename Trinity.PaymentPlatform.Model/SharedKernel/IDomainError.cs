using FluentResults;
using Trinity.PaymentPlatform.Model.Util;

namespace Trinity.PaymentPlatform.Model.SharedKernel;

public interface IDomainError
{
    public int ErrorCode { get; set; } 
    public string ErrorMessage { get; set; }
}

public class DomainError:IDomainError
{
    public int ErrorCode { get; set; }
    public string ErrorMessage { get; set; }

    public DomainError(int errorCode, string message)
    {
        ErrorCode = errorCode;
        ErrorMessage = message;
    }

    public static IDomainError Create(int errorCode, string message) => new DomainError(errorCode, message);

    public static List<IDomainError> CreateList(int errorCode, string message) => new() { Create(errorCode, message) };

    public static List<IDomainError> CreateList(List<IError> errors) =>
        new List<IDomainError>(errors.ConvertAll(p => new DomainError(DefaultErrorCodes.Unspecified, p.Message)));

    public static List<IDomainError> NotFound() => new()
        { Create(DefaultErrorCodes.NotFound, ErrorMessageFormatter.Error("not_found")) };

    public static List<IDomainError> Unauthorized() =>
    [
        Create(DefaultErrorCodes.Unauthorized, ErrorMessageFormatter.Error("unauthorized"))
    ];
    public static List<IDomainError> NotFound(string message) => new()
        { Create(DefaultErrorCodes.NotFound, ErrorMessageFormatter.Error(message)) };
    public static List<IDomainError> UnhandledException() => new()
        { Create(DefaultErrorCodes.Exception, ErrorMessageFormatter.Error()) };
    public static List<IDomainError> Unspecified(string errorMessage) =>
        new() { Create(DefaultErrorCodes.Unspecified, ErrorMessageFormatter.Error(errorMessage)) };

    public static List<IDomainError> Unspecified(List<string> errorMessages) =>
        errorMessages.ConvertAll(p => Create(DefaultErrorCodes.Unspecified, p));

    public static List<IDomainError> Exception(string errorMessage) =>
        new() { Create(DefaultErrorCodes.Exception, errorMessage) };
}

public class DefaultErrorCodes
{
    public const int Unspecified = -1;
    public const int Exception = 0;
    public const int NotFound = 1;
    public const int Unauthorized = 2;

}