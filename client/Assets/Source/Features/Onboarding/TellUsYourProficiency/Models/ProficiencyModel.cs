namespace Source.Features.Onboarding.TellUsYourProficiency.Models
{
    public record ProficiencyModel(string Id, string Name)
    {
        public string Id { get; } = Id;
        public string Name { get; } = Name;
    }
}