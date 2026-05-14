using ErrorOr;

namespace Domain.Users;

public sealed record User
{
    public required string Id { get; init; }
    public required string GuestId { get; init; }
    public DateTime CreatedAt { get; init; }
    public required IReadOnlyList<string> Roles { get; init; } = [];
    public OnboardingState OnboardingState { get; init; }

    public IReadOnlyList<string> RequiredActions
    {
        get
        {
            var actions = new List<string>();

            if (OnboardingState != OnboardingState.Completed)
            {
                actions.Add("CompleteOnboarding");
            }

            return actions;
        }
    }

    public ErrorOr<OnboardingCompleted> Decide(CompleteOnboardingCommand command)
    {
        if (OnboardingState == OnboardingState.Completed)
        {
            return UserErrors.OnboardingAlreadyCompleted;
        }

        return new OnboardingCompleted(
            Id,
            command.Goals.Select(g => g.ToString()).ToList(),
            command.Interests.Select(i => i.ToString()).ToList(),
            command.Proficiency.ToString(),
            command.CompletedAt
        );
    }

    public static UserRegistered Decide(RegisterCommand command) =>
        new(Ulid.NewUlid().ToString(), command.GuestId, Identity.Contracts.Roles.User, command.RegisteredAt);

    public static User Create(UserRegistered @event)
    {
        return new User
        {
            Id = @event.UserId,
            GuestId = @event.GuestId,
            CreatedAt = @event.RegisteredAt,
            Roles = [@event.Role],
            OnboardingState = OnboardingState.NotStarted
        };
    }

    public static User Apply(OnboardingCompleted @event, User user) =>
        user with { OnboardingState = OnboardingState.Completed };
}