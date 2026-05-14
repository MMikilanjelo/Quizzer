namespace Domain.Quizzes;

public sealed record QuizScheduled(string QuizId, string UserId, string Topic, int SequenceNumber, DateTime CreatedAt) : IEvent;

public sealed record QuizContentGenerated(string QuizId, List<QuizQuestion> Questions, DateTime GeneratedAt) : IEvent;

public record QuizCompleted(string QuizId, DateTime CompletedAt) : IEvent;

public sealed record QuizQuestionAnswered(
    string QuizId,
    string UserId,
    string ConceptId,
    string QuestionId,
    int SelectedIndex,
    bool IsCorrect,
    DateTime AnsweredAt
) : IEvent;