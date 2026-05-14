using Application.Users.Views;
using Domain.Users;
using Marten.Events.Aggregation;

namespace Infrastructure.Projections;

public class UserProfileViewProjection : SingleStreamProjection<UserProfileView, string>
{
    public UserProfileView Create(UserRegistered @event) =>
        new()
        {
            Id = @event.UserId,
            OnboardingState = OnboardingState.NotStarted,
            Goals = [],
            Interests = [],
            Proficiency = null
        };

    public UserProfileView Apply(OnboardingCompleted @event, UserProfileView current) =>
        current with
        {
            OnboardingState = OnboardingState.Completed,
            Goals = @event.Goals.Select(Enum.Parse<Goal>).ToList(),
            Interests = @event.Interests.Select(Enum.Parse<Interest>).ToList(),
            Proficiency = Enum.Parse<Proficiency>(@event.ProficiencyLevel)
        };
}