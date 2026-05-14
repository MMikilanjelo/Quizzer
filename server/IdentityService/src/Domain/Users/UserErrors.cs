using ErrorOr;

namespace Domain.Users;

public static class UserErrors
{
    public static readonly Error DeviceAlreadyLinked =
        Error.Conflict("User.DeviceAlreadyLinked", "This device is already linked to an existing account.");

    public static readonly Error NotFound =
        Error.NotFound("User.NotFound", "User not found.");

    public static readonly Error OnboardingAlreadyCompleted =
        Error.Conflict("User.OnboardingCompleted", "User has already completed the onboarding process.");
}