namespace Application.Quizzes.Views;

public sealed record UserDashboardView
{
    public required string Id { get; set; }
    public float AverageScore { get; set; }
    public int TotalQuizzes { get; set; }
    public int PerfectQuizzes { get; set; }
    public int TotalQuestionsAnswered { get; init; }
    public int TotalCorrectAnswers { get; init; }
    public int StreakDays { get; set; }
    public DateTime? LastQuizDate { get; set; }
    public IReadOnlyList<TopicMasteryLevelView> TopicMasteryLevels { get; init; } = [];
    public IReadOnlyList<ConceptPerformanceView> ConceptPerformance { get; init; } = [];
    public ICollection<TopicMasteryLevelView> TopStrengths => TopicMasteryLevels.OrderByDescending(x => x.MasteryPercentage).Take(3).ToList();
    public ICollection<TopicMasteryLevelView> FocusAreas => TopicMasteryLevels.OrderBy(x => x.MasteryPercentage).Take(2).ToList();
}

public sealed record TopicMasteryLevelView
{
    public required string TopicId { get; init; }
    public required int MasteryPercentage { get; init; }
}

public record ConceptPerformanceView
{
    public required string ConceptId { get; init; }
    public int TotalAnswered { get; init; }
    public int TotalCorrect { get; init; }
    public float RawAccuracy => TotalAnswered == 0 ? 0f : (float)TotalCorrect / TotalAnswered;
}