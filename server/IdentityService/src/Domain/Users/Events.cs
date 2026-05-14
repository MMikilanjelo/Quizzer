namespace Domain.Users;

public record OnboardingCompleted(
    string UserId,
    List<string> Goals,
    List<string> Interests,
    string ProficiencyLevel,
    DateTime CompletedAt
);

public sealed record UserRegistered(
    string UserId,
    string GuestId,
    string Role,
    DateTime RegisteredAt
);