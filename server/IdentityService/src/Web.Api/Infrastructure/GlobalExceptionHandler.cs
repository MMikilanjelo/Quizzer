using ErrorOr;
using JasperFx;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Web.Api.Infrastructure;

internal sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var error = exception switch
        {
            UnauthorizedAccessException => Error.Unauthorized(
                "Unauthorized",
                exception.Message),

            ConcurrencyException => Error.Conflict(
                "Concurrency.Conflict",
                "The resource was modified by another process. Please try again."),

            _ => Error.Failure(
                "Server.Failure",
                "An unexpected error occurred.")
        };

        if (error.Type == ErrorType.Failure)
        {
            logger.LogError(exception, "Unhandled server exception occurred: {Message}", exception.Message);
        }
        else
        {
            logger.LogWarning("Client/State exception occurred: {Type} - {Message}", exception.GetType().Name, exception.Message);
        }

        var result = CustomResults.Problem(error);

        await result.ExecuteAsync(httpContext);

        return true;
    }
}