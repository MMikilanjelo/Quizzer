using Domain.Users;

namespace Application.Users.Views;

public sealed record UserProfileView
{
    public required string Id { get; set; }
    public OnboardingState OnboardingState { get; set; }
    public List<Goal> Goals { get; set; } = [];
    public List<Interest> Interests { get; set; } = [];
    public Proficiency? Proficiency { get; set; }
}