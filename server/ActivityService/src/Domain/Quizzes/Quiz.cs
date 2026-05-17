using System.Collections.Immutable;
using ErrorOr;

namespace Domain.Quizzes;

public sealed record QuizQuestion(string Id, string TopicId, string Text, List<string> Options, int CorrectIndex);

public sealed record Quiz
{
    public const int MinQuestionCount = 5;

    public const int MaxQuestionCount = 50;

    public enum QuizStatus
    {
        Pending,
        InProgress,
        Ready,
        Completed
    }

    public enum ScheduleType
    {
        Manual,
        Smart
    }

    public enum DifficultyLevel
    {
        Unspecified,
        Easy,
        Medium,
        Hard
    }

    public required string Id { get; init; }
    public required string UserId { get; init; }
    public required string DomainId { get; init; }
    public required int SequenceNumber { get; init; }
    public required int? DesiredQuestionsCount { get; init; }
    public required QuizStatus Status { get; init; }
    public required ScheduleType Schedule { get; init; }
    public required DifficultyLevel UserDifficulty { get; init; }
    public required DifficultyLevel SystemDifficulty { get; init; }
    public required IReadOnlyList<QuizQuestion> Questions { get; init; }
    public required IReadOnlyList<string> AnsweredQuestionIds { get; init; }
    public required IReadOnlyList<string> CorrectQuestionIds { get; init; }
    public required DateTime CreatedAt { get; init; }
    public DateTime? CompletedAt { get; init; }
    public bool IsPerfect => Questions.Count != 0 && CorrectQuestionIds.Count == Questions.Count;
    public float ScorePercentage => Questions.Count > 0 ? (float)CorrectQuestionIds.Count / Questions.Count : 0f;

    public ErrorOr<QuizContentGenerated> Fill(FillQuizCommand command)
    {
        if (Status != QuizStatus.Pending)
        {
            return QuizErrors.NotPending;
        }

        if (command.Questions.Count == 0)
        {
            return QuizErrors.EmptyQuestions;
        }

        return new QuizContentGenerated
        {
            QuizId = Id,
            Questions = command.Questions.ToList(),
            SystemDifficulty = command.SystemDifficulty,
            GeneratedAt = command.GeneratedAt
        };
    }

    public ErrorOr<IReadOnlyList<IEvent>> AnswerQuestion(AnswerQuestionCommand command)
    {
        if (UserId != command.AttemptingUserId)
        {
            return QuizErrors.Forbidden;
        }

        if (Status is not (QuizStatus.Ready or QuizStatus.InProgress))
        {
            return QuizErrors.NotActive;
        }

        if (AnsweredQuestionIds.Contains(command.QuestionId))
        {
            return QuizErrors.AlreadyAnswered;
        }

        var question = Questions.FirstOrDefault(q => q.Id == command.QuestionId);

        if (question is null)
        {
            return QuizErrors.QuestionNotFound;
        }

        var isCorrect = question.CorrectIndex == command.SelectedIndex;

        var events = new List<IEvent>
        {
            new QuizQuestionAnswered
            {
                QuizId = Id,
                UserId = UserId,
                ConceptId = question.TopicId,
                QuestionId = command.QuestionId,
                SelectedIndex = command.SelectedIndex,
                IsCorrect = isCorrect,
                AnsweredAt = command.AnsweredAt
            }
        };

        var answeredCount = AnsweredQuestionIds.Count + 1;

        if (Status == QuizStatus.Completed || answeredCount < Questions.Count)
        {
            return events;
        }

        var finalCorrectCount = CorrectQuestionIds.Count + (isCorrect ? 1 : 0);

        var finalIsPerfect = finalCorrectCount == Questions.Count;

        var finalScore = Questions.Count > 0 ? (float)finalCorrectCount / Questions.Count : 0f;

        events.Add(new QuizCompleted
        {
            QuizId = Id,
            UserId = UserId,
            IsPerfect = finalIsPerfect,
            ScorePercentage = finalScore,
            CompletedAt = command.AnsweredAt
        });

        return events;
    }

    public static Quiz Create(SmartQuizScheduled @event)
    {
        return new Quiz
        {
            Id = @event.QuizId,
            UserId = @event.UserId,
            DomainId = @event.DomainId,
            Status = QuizStatus.Pending,
            Schedule = ScheduleType.Smart,
            SequenceNumber = @event.SequenceNumber,
            Questions = [],
            DesiredQuestionsCount = null,
            AnsweredQuestionIds = [],
            CorrectQuestionIds = [],
            SystemDifficulty = DifficultyLevel.Unspecified,
            UserDifficulty = DifficultyLevel.Unspecified,
            CreatedAt = @event.CreatedAt,
            CompletedAt = null
        };
    }

    public static Quiz Create(ManualQuizScheduled @event)
    {
        return new Quiz
        {
            Id = @event.QuizId,
            UserId = @event.UserId,
            DomainId = @event.DomainId,
            Status = QuizStatus.Pending,
            Schedule = ScheduleType.Manual,
            SequenceNumber = @event.SequenceNumber,
            DesiredQuestionsCount = @event.QuestionCount,
            Questions = [],
            AnsweredQuestionIds = [],
            CorrectQuestionIds = [],
            SystemDifficulty = DifficultyLevel.Unspecified,
            UserDifficulty = @event.DifficultyLevel,
            CreatedAt = @event.CreatedAt,
            CompletedAt = null
        };
    }

    public Quiz Apply(QuizQuestionAnswered @event)
    {
        return this with
        {
            Status = QuizStatus.InProgress,
            AnsweredQuestionIds = [.. AnsweredQuestionIds, @event.QuestionId],
            CorrectQuestionIds = @event.IsCorrect
                ? [.. CorrectQuestionIds, @event.QuestionId]
                : CorrectQuestionIds
        };
    }

    public Quiz Apply(QuizContentGenerated @event)
    {
        return this with
        {
            Questions = @event.Questions.ToList(),
            SystemDifficulty = @event.SystemDifficulty,
            Status = QuizStatus.Ready
        };
    }

    public Quiz Apply(QuizCompleted @event)
    {
        return this with
        {
            Status = QuizStatus.Completed,
            CompletedAt = @event.CompletedAt
        };
    }
}