using System.Net;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Trinity.PaymentPlatform.Model.SharedKernel;
using Trinity.PaymentProvider.API.Shared.ActionResults;

namespace Trinity.PaymentProvider.API.Shared.Middleware;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        _logger.LogError(exception, exception.Message);
#if DEBUG
        string contents = DateTime.Now + Environment.NewLine;

        Exception? ex = exception;

        while (ex != null)
        {
            contents += ex.Message + Environment.NewLine;
            contents += ex.StackTrace + Environment.NewLine;

            ex = ex.InnerException;
        }
        var problemDetails = new ProblemDetails()
        {
            Status = (int)HttpStatusCode.InternalServerError,
            Detail = contents,
            Title = "Unhandled exception.",
        };

        GenericResponse<object> response = GenericResponse<object>.ToError(DomainError.Exception(contents));
        httpContext.Response.ContentType = "application/json";
        httpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        await httpContext.Response.WriteAsJsonAsync(response, cancellationToken);
#else
        GenericResponse<object> response = GenericResponse<object>.ToError(DomainError.UnhandledException());
        httpContext.Response.ContentType = "application/json";
        httpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        await httpContext.Response.WriteAsJsonAsync(response, cancellationToken);
#endif


        return true;
    }
}