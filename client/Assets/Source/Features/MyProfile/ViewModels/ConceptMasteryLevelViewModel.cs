namespace Source.Features.MyProfile.ViewModels
{
    public sealed record ConceptMasteryLevelViewModel
    {
        public string ConceptId { get; set; }
        public int MasteryPercentage { get; set; }
    }
}