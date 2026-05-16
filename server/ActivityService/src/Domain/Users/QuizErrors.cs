using ErrorOr;

namespace Domain.Users;

public static class UserErrors
{
    public static readonly Error NotFound =
        Error.NotFound("User.NotFound", "User not found.");
}