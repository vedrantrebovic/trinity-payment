using FluentResults;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using IError = FluentResults.IError;

namespace Trinity.PaymentProvider.API.Shared.ActionResults;

/// <summary>
/// API response container.
/// </summary>
/// <typeparam name="T">Return object.</typeparam>
public  class Response<T>
{
    #region Properties

    /// <summary>List of Errors</summary>
    public List<Error> Errors { get; set; }
    public T Result { get; set; }

    #endregion Properties

    #region Ctor

    public Response()
    {
        Errors = new List<Error>();
    }

    public Response(T result)
    {
        Errors = new List<Error>();
        Result = result;
    }

    public Response(T result, List<Error> errors)
    {
        Errors = errors;
        Result = result;
    }

    public Response(IEnumerable<Error> errors)
    {
        Errors = new List<Error>(errors);
    }

    #endregion Ctor

}

public sealed class Response : Response<object>
{
    private Response(IEnumerable<Error> errors) : base(errors) { }
    internal static Response<T> Ok<T>(T result)
    {
        return new Response<T>(result);
    }

    internal static IActionResult Result<T>(Result<T> result)
    {
        if(result.IsFailed) return new BadRequestObjectResult(Error(result.Errors));
        return new OkObjectResult(Ok(result.Value));
    }

    public static IActionResult Result(FluentResults.Result result)
    {
        if(result.IsFailed) return new BadRequestObjectResult(Error(result.Errors));
        return new OkObjectResult(Ok(result.IsSuccess));
    }

    internal static Response Error(IEnumerable<ValidationFailure> failures)
    {
        return new Response(failures.ToList().ConvertAll(p => new Error(p.ErrorCode, p.ErrorMessage)));
    }

    internal static Response Error(IEnumerable<IError> errors)
    {
        IList<Error> err = new List<Error>();
        foreach (var error in errors)
        {
            err.Add(new Error(error.Message, error.Message));
        }
        return new Response(err);
    }
}

public class Error
{
    public string ErrorCode { get; set; }
    public string ErrorMessage { get; set; }

    public Error(string errorCode, string errorMessage)
    {
        ErrorCode = errorCode;
        ErrorMessage = errorMessage;
    }
}

public static class ControllerExtension
{
    public static IActionResult Result<T>(this ControllerBase controllerBase, Result<T> result)
    {
        return Response.Result(result);
    }
    
    public static IActionResult Result(this ControllerBase controllerBase, Result result)
    {
        return Response.Result(result);
    }
}