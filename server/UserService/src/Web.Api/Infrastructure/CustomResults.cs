using ErrorOr;

namespace Web.Api.Infrastructure;

public static class CustomResults
{
    public static IResult Problem(List<Error> errors)
    {
        if (errors.Count == 0)
        {
            return Results.Problem();
        }

        var firstError = errors[0];

        return Results.Problem(
            title: GetTitle(firstError),
            detail: firstError.Description,
            type: GetType(firstError.Type),
            statusCode: GetStatusCode(firstError.Type),
            extensions: GetErrors(errors));

        static string GetTitle(Error error) =>
            error.Type switch
            {
                ErrorType.Validation => "Validation Error",
                ErrorType.Conflict => "Conflict Error",
                ErrorType.NotFound => "Not Found Error",
                ErrorType.Unauthorized => "Unauthorized Error",
                _ => error.Code
            };

        static string GetType(ErrorType errorType) =>
            errorType switch
            {
                ErrorType.Validation => "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                ErrorType.NotFound => "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                ErrorType.Conflict => "https://tools.ietf.org/html/rfc7231#section-6.5.8",
                ErrorType.Unauthorized => "https://tools.ietf.org/html/rfc7231#section-6.5.7",
                _ => "https://tools.ietf.org/html/rfc7231#section-6.6.1"
            };

        static int GetStatusCode(ErrorType errorType) =>
            errorType switch
            {
                ErrorType.Validation => StatusCodes.Status400BadRequest,
                ErrorType.NotFound => StatusCodes.Status404NotFound,
                ErrorType.Conflict => StatusCodes.Status409Conflict,
                ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
                ErrorType.Forbidden => StatusCodes.Status403Forbidden,
                _ => StatusCodes.Status500InternalServerError
            };

        static Dictionary<string, object?>? GetErrors(List<Error> errors)
        {
            if (errors.Count == 1 && errors[0].Type != ErrorType.Validation)
            {
                return null;
            }

            return new Dictionary<string, object?>
            {
                { "errorCodes", errors.Select(e => e.Code) }
            };
        }
    }
}