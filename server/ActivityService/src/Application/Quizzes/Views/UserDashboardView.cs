namespace Application.Quizzes.Views;

public sealed record UserDashboardView
{
    public required string Id { get; set; }
    public float AverageScore { get; set; }
    public int TotalQuizzes { get; set; }
    public int PerfectQuizzes { get; set; }
    public int StreakDays { get; set; }
    public DateTime? LastQuizDate { get; set; }
    public List<ConceptMasteryLevelView> ConceptMasteryLevels { get; set; } = [];

    public ICollection<ConceptMasteryLevelView> TopStrengths =>
        ConceptMasteryLevels.OrderByDescending(x => x.MasteryPercentage).Take(3).ToList();

    public ICollection<ConceptMasteryLevelView> FocusAreas =>
        ConceptMasteryLevels.OrderBy(x => x.MasteryPercentage).Take(2).ToList();
}

public sealed record ConceptMasteryLevelView
{
    public required string ConceptId { get; init; }
    public required int MasteryPercentage { get; init; }
}