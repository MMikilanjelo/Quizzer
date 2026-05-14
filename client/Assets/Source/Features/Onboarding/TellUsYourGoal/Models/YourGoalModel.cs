namespace Source.Features.Onboarding.TellUsYourGoal.Models
{
    public record YourGoalModel(string Id, string Name)
    {
        public string Id { get; } = Id;
        public string Name { get; } = Name;
    }
}