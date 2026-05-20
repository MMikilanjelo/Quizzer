namespace Domain.Learning;

public record TopicMastery
{
    public required string Id { get; init; }
    public required string UserId { get; init; }
    public required string TopicId { get; init; }
    public required Mastery Mastery { get; init; }
    public required BktParams Params { get; init; }
    public DateTime LastUpdated { get; init; }

    public static TopicMastery Create(TopicMasteryStarted @event)
    {
        return new TopicMastery
        {
            Id = FormatId(@event.UserId, @event.TopicId),
            UserId = @event.UserId,
            TopicId = @event.TopicId,
            Mastery = Mastery.Initial,
            Params = new BktParams
            {
                PTransition = @event.PTransition
            },
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

    public TopicMasteryUpdated RecordAttempt(
        bool isCorrect,
        string quizId,
        string questionId,
        QuestionDynamics dynamics,
        DateTime timestamp
    )
    {
        var nextMastery = Mastery.CalculateNext(isCorrect, Params, dynamics);

        return new TopicMasteryUpdated
        {
            UserId = UserId,
            TopicId = TopicId,
            QuizId = quizId,
            QuestionId = questionId,
            OldMastery = Mastery.Value,
            NewMastery = nextMastery.Value,
            PGuess = dynamics.PGuess,
            PSlip = dynamics.PSlip,
            PTransition = Params.PTransition,
            Timestamp = timestamp
        };
    }

    public static string FormatId(string userId, string topicId) => $"{userId}:{topicId}";
}