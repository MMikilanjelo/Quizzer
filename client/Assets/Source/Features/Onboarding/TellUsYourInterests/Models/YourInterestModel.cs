namespace Source.Features.Onboarding.TellUsYourInterests.Models
{
    public record YourInterestModel(string Id, string Name)
    {
        public string Id { get; } = Id;
        public string Name { get; } = Name;
    }
}