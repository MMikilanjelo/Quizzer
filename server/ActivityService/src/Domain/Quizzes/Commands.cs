using System.Collections.Immutable;

namespace Domain.Quizzes;

public sealed record FillQuizCommand
{
    public required ImmutableList<QuizQuestion> Questions { get; init; }
    public required Quiz.DifficultyLevel Difficulty { get; init; }
    public required DateTime GeneratedAt { get; init; }
}

public sealed record AnswerQuestionCommand
{
    public required string QuestionId { get; init; }
    public required int SelectedIndex { get; init; }
    public required string AttemptingUserId { get; init; }
    public required DateTime AnsweredAt { get; init; }
}