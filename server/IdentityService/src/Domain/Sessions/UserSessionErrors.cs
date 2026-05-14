using ErrorOr;

namespace Domain.Sessions;

public static class UserSessionErrors
{
    public static readonly Error SessionExpired = Error.Unauthorized(
        code: "Identity.SessionExpired",
        description: "Session is expired or revoked. Please log in again."
    );

    public static readonly Error RefreshTokenInvalid = Error.Unauthorized(
        code: "Identity.InvalidRefreshToken",
        description: "Invalid refresh token. Please log in again."
    );
}