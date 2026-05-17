namespace Domain.Learning;

public record TopicMastery
{
    public required string Id { get; init; }
    public required string UserId { get; init; }
    public required string ConceptId { get; init; }
    public required Mastery Mastery { get; init; }
    public required BktParams Params { get; init; }
    public DateTime LastUpdated { get; init; }

    public static TopicMastery Create(TopicMasteryStarted @event)
    {
        return new TopicMastery
        {
            Id = FormatId(@event.UserId, @event.TopicId),
            UserId = @event.UserId,
            ConceptId = @event.TopicId,
            Mastery = Mastery.Initial,
            Params = @event.InitialParams,
            LastUpdated = @event.StartedAt
        };
    }

    public TopicMastery Apply(TopicMasteryUpdated @event)
    {
        return this with
        {
            Mastery = new Mastery { Value = @event.NewMastery },
            LastUpdated = @event.Timestamp
        };
    }

    public TopicMasteryUpdated RecordAttempt(bool isCorrect, string quizId, string questionId, DateTime timestamp)
    {
        var nextMastery = Mastery.CalculateNext(isCorrect, Params);

        return new TopicMasteryUpdated(
            UserId, ConceptId, quizId, questionId,
            Mastery.Value, nextMastery.Value,
            Params.PGuess, Params.PSlip, Params.PTransition,
            timestamp
        );
    }

    public static string FormatId(string userId, string topicId) => $"{userId}:{topicId}";
}