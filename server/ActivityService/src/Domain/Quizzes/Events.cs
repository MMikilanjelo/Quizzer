namespace Domain.Quizzes;

public sealed record SmartQuizScheduled : IEvent
{
    public required string QuizId { get; init; }
    public required string UserId { get; init; }
    public required string TopicId { get; init; }
    public required int SequenceNumber { get; init; }
    public required DateTime CreatedAt { get; init; }
}

public sealed record ManualQuizScheduled : IEvent
{
    public required string QuizId { get; init; }
    public required string UserId { get; init; }
    public required string TopicId { get; init; }
    public required int QuestionCount { get; init; }
    public required string DifficultyLevelId { get; init; }
    public required int SequenceNumber { get; init; }
    public required DateTime CreatedAt { get; init; }
}

public sealed record QuizContentGenerated : IEvent
{
    public required string QuizId { get; init; }
    public required List<QuizQuestion> Questions { get; init; }
    public required Quiz.DifficultyLevels SystemDifficulty { get; init; }
    public required DateTime GeneratedAt { get; init; }
}

public sealed record QuizQuestionAnswered : IEvent
{
    public required string QuizId { get; init; }
    public required string UserId { get; init; }
    public required string ConceptId { get; init; }
    public required string QuestionId { get; init; }
    public required int SelectedIndex { get; init; }
    public required bool IsCorrect { get; init; }
    public required DateTime AnsweredAt { get; init; }
}

public sealed record QuizCompleted : IEvent
{
    public required string QuizId { get; init; }
    public required string UserId { get; init; }
    public required bool IsPerfect { get; init; }
    public required float ScorePercentage { get; init; }
    public required DateTime CompletedAt { get; init; }
}