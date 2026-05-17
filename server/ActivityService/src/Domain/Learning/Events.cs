namespace Domain.Learning;

public record TopicMasteryUpdated(
    string UserId,
    string TopicId,
    string QuizId,
    string QuestionId,
    double OldMastery,
    double NewMastery,
    double PGuess,
    double PSlip,
    double PTransition,
    DateTime Timestamp
) : IEvent;

public record TopicMasteryStarted(
    string UserId,
    string TopicId,
    BktParams InitialParams,
    DateTime StartedAt
) : IEvent;