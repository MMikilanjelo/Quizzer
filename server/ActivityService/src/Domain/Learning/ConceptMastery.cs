namespace Domain.Learning;

public record ConceptMastery
{
    public required string Id { get; init; }
    public required string UserId { get; init; }
    public required string ConceptId { get; init; }
    public required Mastery Mastery { get; init; }
    public required BktParams Params { get; init; }
    public DateTime LastUpdated { get; init; }

    public static ConceptMastery Create(ConceptMasteryStarted @event)
    {
        return new ConceptMastery
        {
            Id = FormatId(@event.UserId, @event.ConceptId),
            UserId = @event.UserId,
            ConceptId = @event.ConceptId,
            Mastery = Mastery.Initial,
            Params = @event.InitialParams,
            LastUpdated = @event.StartedAt
        };
    }

    public ConceptMastery Apply(ConceptMasteryUpdated @event)
    {
        return this with
        {
            Mastery = new Mastery { Value = @event.NewMastery },
            LastUpdated = @event.Timestamp
        };
    }

    public ConceptMasteryUpdated RecordAttempt(bool isCorrect, string quizId, string questionId, DateTime timestamp)
    {
        var nextMastery = Mastery.CalculateNext(isCorrect, Params);

        return new ConceptMasteryUpdated(
            UserId, ConceptId, quizId, questionId,
            Mastery.Value, nextMastery.Value,
            Params.PGuess, Params.PSlip, Params.PTransition,
            timestamp
        );
    }

    public static string FormatId(string userId, string conceptId) => $"{userId}:{conceptId}";
}