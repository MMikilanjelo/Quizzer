namespace Domain.Users;

public sealed record CompleteOnboardingCommand(
    List<Goal> Goals,
    List<Interest> Interests,
    Proficiency Proficiency,
    DateTime CompletedAt
);

public sealed record RegisterCommand(
    string GuestId,
    DateTime RegisteredAt
);