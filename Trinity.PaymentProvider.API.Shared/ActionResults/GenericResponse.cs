using MessagePack;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OneOf;
using OneOf.Types;
using Trinity.PaymentPlatform.Model.SharedKernel;

namespace Trinity.PaymentProvider.API.Shared.ActionResults;

[MessagePackObject]
public class GenericResponse<T>
{
    public GenericResponse(IList<IDomainError>? errors, T result)
    {
        Errors = errors;
        Result = result;
    }

    private GenericResponse(){}

    private GenericResponse(IList<IDomainError>? errors)
    {
        Errors = errors;
    }

    [Key("errors")]
    public IList<IDomainError>? Errors { get; set; }
    [Key("result")]
    public T Result { get; set; }

    public static GenericResponse<T> ToResult(T result)
    {
        return new GenericResponse<T>(null, result);
    }

    public static GenericResponse<object> ToError(IList<IDomainError> errors) => new(errors);

    public static GenericResponse<T> NotFound(IList<IDomainError> errors) => new(errors);
}

public static class GenericResponseExtensions
{
    public static IActionResult Result<T>(this ControllerBase controller, OneOf<T, List<IDomainError>> result)
    {
        return result.Match<IActionResult>(arg => new OkObjectResult(GenericResponse<T>.ToResult(arg)),
            list =>new BadRequestObjectResult(GenericResponse<T>.ToError(list)));
    }

    public static IActionResult Result<T>(this ControllerBase controller, OneOf<T, None, List<IDomainError>, Exception> result)
    {
        return result.Match(arg => new OkObjectResult(GenericResponse<T>.ToResult(arg)),
            none =>
                new NotFoundObjectResult(GenericResponse<T>.NotFound(DomainError.NotFound())),
            list => new BadRequestObjectResult(GenericResponse<T>.ToError(list)),
            exception => new ObjectResult(GenericResponse<T>.ToError(DomainError.UnhandledException()))
                { StatusCode = StatusCodes.Status500InternalServerError });
    }

    public static IActionResult Result<T>(this ControllerBase controller, T value)
    {
        return new OkObjectResult(GenericResponse<T>.ToResult(value));
    }
}