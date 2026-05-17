namespace Application.Quizzes.Views;

public sealed record QuizSummaryView
{
    public enum QuizStatus
    {
        Ready,
        InProgress,
        Pending,
        Completed
    }

    public required string Id { get; set; }
    public required string UserId { get; set; }
    public required List<string> Topics { get; set; }
    public required string Name { get; set; }
    public required QuizStatus Status { get; set; }
    public int QuestionCount { get; init; }
    public int AnsweredCount { get; init; }
    public DateTime CreatedAt { get; set; }
    public required List<QuestionView> Questions { get; set; }
    public required List<TopicMasteryDeltaView> MasteryChanges { get; set; }
}

public sealed record QuestionView
{
    public required string Id { get; set; }
    public required string TopicId { get; set; }
    public required string Text { get; set; }
    public required List<string> Options { get; set; }
    public int CorrectAnswerIndex { get; set; }
    public int? SelectedAnswerIndex { get; set; }
}

public sealed record TopicMasteryDeltaView
{
    public required string TopicId { get; init; }
    public required double StartingMastery { get; init; }
    public required double EndingMastery { get; init; }
    public required int AttemptsDuringQuiz { get; init; }
}