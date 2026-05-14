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
    public required DateTime CreatedAt { get; init; }
    public DateTime? CompletedAt { get; init; }

    public ErrorOr<QuizContentGenerated> Fill(ImmutableList<QuizQuestion> questions, DateTime generatedAt)
    {
        if (Status != QuizStatus.Pending)
        {
            return QuizErrors.NotPending;
        }

        if (questions.Count == 0)
        {
            return QuizErrors.EmptyQuestions;
        }

        return new QuizContentGenerated(Id, questions.ToList(), generatedAt);
    }

    public ErrorOr<IReadOnlyList<IEvent>> AnswerQuestion(
        string questionId,
        int selectedIndex,
        string attemptingUserId,
        DateTime answeredAt)
    {
        if (UserId != attemptingUserId)
        {
            return QuizErrors.Forbidden;
        }

        if (Status is not (QuizStatus.Ready or QuizStatus.InProgress))
        {
            return QuizErrors.NotActive;
        }

        if (AnsweredQuestionIds.Contains(questionId))
        {
            return QuizErrors.AlreadyAnswered;
        }

        var question = Questions.FirstOrDefault(q => q.Id == questionId);

        if (question is null)
        {
            return QuizErrors.QuestionNotFound;
        }

        var isCorrect = question.CorrectIndex == selectedIndex;

        var events = new List<IEvent>
        {
            new QuizQuestionAnswered(
                Id,
                UserId,
                question.ConceptId,
                questionId,
                selectedIndex,
                isCorrect,
                answeredAt)
        };

        var answeredCount = AnsweredQuestionIds.Count + 1;

        if (Status != QuizStatus.Completed &&
            answeredCount >= Questions.Count)
        {
            events.Add(new QuizCompleted(Id, answeredAt));
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
            CreatedAt = @event.CreatedAt,
            CompletedAt = null
        };
    }

    public Quiz Apply(QuizQuestionAnswered @event)
    {
        return this with
        {
            Status = QuizStatus.InProgress,
            AnsweredQuestionIds = [.. AnsweredQuestionIds, @event.QuestionId]
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