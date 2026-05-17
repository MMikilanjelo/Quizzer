namespace Application.Quizzes.Views;

public sealed record UserDashboardView
{
    public required string Id { get; set; }
    public float AverageScore { get; set; }
    public int TotalQuizzes { get; set; }
    public int PerfectQuizzes { get; set; }
    public int StreakDays { get; set; }
    public DateTime? LastQuizDate { get; set; }
    public List<TopicMasteryLevelView> TopicMasteryLevels { get; set; } = [];
    public ICollection<TopicMasteryLevelView> TopStrengths => TopicMasteryLevels.OrderByDescending(x => x.MasteryPercentage).Take(3).ToList();
    public ICollection<TopicMasteryLevelView> FocusAreas => TopicMasteryLevels.OrderBy(x => x.MasteryPercentage).Take(2).ToList();
}

public sealed record TopicMasteryLevelView
{
    public required string TopicId { get; init; }
    public required int MasteryPercentage { get; init; }
}