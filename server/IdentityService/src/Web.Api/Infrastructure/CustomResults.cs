using ErrorOr;

namespace Web.Api.Infrastructure;

public static class CustomResults
{
    public static IResult Problem(Error error) =>
        Problem([error]);

    public static IResult Problem(List<Error> errors)
    {
        if (errors.Count == 0)
        {
            return Results.Problem();
        }

        var firstError = errors[0];

        var statusCode = GetStatusCode(firstError.Type);

        var detail = firstError.Type switch
        {
            ErrorType.Validation => "One or more validation requirements were not met.",
            ErrorType.Conflict => "The request could not be completed due to a state conflict.",
            ErrorType.NotFound => "The requested resource was not found.",
            ErrorType.Unauthorized => "Authentication is required to access this resource.",
            _ => "An unexpected error occurred during processing."
        };

        var extensions = new Dictionary<string, object?>
        {
            {
                "errors", errors.Select(e => new
                {
                    code = e.Code,
                    message = e.Description
                }).ToArray()
            }
        };

        return Results.Problem(
            detail: detail,
            statusCode: statusCode,
            extensions: extensions
        );

        static int GetStatusCode(ErrorType errorType) =>
            errorType switch
            {
                ErrorType.NotFound => StatusCodes.Status404NotFound,
                ErrorType.Conflict => StatusCodes.Status409Conflict,
                ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
                ErrorType.Forbidden => StatusCodes.Status403Forbidden,
                ErrorType.Validation => StatusCodes.Status400BadRequest,
                _ => StatusCodes.Status500InternalServerError
            };
    }
}