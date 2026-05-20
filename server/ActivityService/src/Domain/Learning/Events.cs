namespace Domain.Learning;

public sealed record TopicMasteryUpdated : IEvent
{
    public required string UserId { get; init; }
    public required string TopicId { get; init; }
    public required string QuizId { get; init; }
    public required string QuestionId { get; init; }
    public required double OldMastery { get; init; }
    public required double NewMastery { get; init; }
    public required double PGuess { get; init; }
    public required double PSlip { get; init; }
    public required double PTransition { get; init; }
    public required DateTime Timestamp { get; init; }
}

public sealed record TopicMasteryStarted : IEvent
{
    public required string UserId { get; init; }
    public required string TopicId { get; init; }
    public required double PTransition { get; init; }
    public required DateTime StartedAt { get; init; }
}