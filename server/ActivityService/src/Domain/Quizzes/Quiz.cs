using System.Collections.Immutable;
using ErrorOr;

namespace Domain.Quizzes;

public sealed record QuizQuestion(string Id, string ConceptId, string Text, List<string> Options, int CorrectIndex);

public sealed record Quiz
{
    public enum QuizStatus
    {
        Pending = 0,
        InProgress = 1,
        Ready = 2,
        Completed = 3
    }

    public required string Id { get; init; }
    public required string UserId { get; init; }
    public required string Topic { get; init; }
    public required int SequenceNumber { get; init; }
    public required QuizStatus Status { get; init; }
    public required IReadOnlyList<QuizQuestion> Questions { get; init; }
    public required IReadOnlyList<string> AnsweredQuestionIds { get; init; }
    public required IReadOnlyList<string> CorrectQuestionIds { get; init; }
    public required DateTime CreatedAt { get; init; }
    public DateTime? CompletedAt { get; init; }
    public bool IsPerfect => CorrectQuestionIds.Count == Questions.Count;

    public float ScorePercentage => Questions.Count > 0
        ? (float)CorrectQuestionIds.Count / Questions.Count
        : 0f;

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
                ConceptId = question.ConceptId,
                QuestionId = command.QuestionId,
                SelectedIndex = command.SelectedIndex,
                IsCorrect = isCorrect,
                AnsweredAt = command.AnsweredAt
            }
        };

        var answeredCount = AnsweredQuestionIds.Count + 1;

        if (Status != QuizStatus.Completed && answeredCount >= Questions.Count)
        {
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
        }

        return events;
    }

    public static Quiz Create(QuizScheduled @event)
    {
        return new Quiz
        {
            Id = @event.QuizId,
            UserId = @event.UserId,
            Topic = @event.Topic,
            Status = QuizStatus.Pending,
            SequenceNumber = @event.SequenceNumber,
            Questions = [],
            AnsweredQuestionIds = [],
            CorrectQuestionIds = [],
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