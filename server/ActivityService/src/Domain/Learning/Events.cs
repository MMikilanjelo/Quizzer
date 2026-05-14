namespace Domain.Learning;

public record ConceptMasteryUpdated(
    string UserId,
    string ConceptId,
    string QuizId,
    string QuestionId,
    double OldMastery,
    double NewMastery,
    double PGuess,
    double PSlip,
    double PTransition,
    DateTime Timestamp
) : IEvent;

public record ConceptMasteryStarted(
    string UserId,
    string ConceptId,
    BktParams InitialParams,
    DateTime StartedAt
) : IEvent;