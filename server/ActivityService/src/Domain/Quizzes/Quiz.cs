using System.Collections.Immutable;
using ErrorOr;

namespace Domain.Quizzes;

public sealed record QuizQuestion
{
    public required string Id { get; init; }
    public required string TopicId { get; init; }
    public required string Text { get; init; }
    public required IReadOnlyCollection<string> Options { get; init; }
    public required int CorrectIndex { get; init; }
    public int? SelectedIndex { get; init; }
    public bool IsAnswered => SelectedIndex.HasValue;
    public bool IsCorrect => IsAnswered && SelectedIndex == CorrectIndex;
}

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
    public required DifficultyLevel Difficulty { get; init; }
    public required IReadOnlyList<QuizQuestion> Questions { get; init; }
    public required DateTime CreatedAt { get; init; }
    public DateTime? CompletedAt { get; init; }

    // Computed properties
    public int AnsweredCount => Questions.Count(q => q.IsAnswered);
    public int CorrectCount => Questions.Count(q => q.IsCorrect);
    public bool IsPerfect => Questions.Count > 0 && Questions.All(q => q.IsCorrect);

    public float ScorePercentage
    {
        get
        {
            var hasQuestions = Questions.Count > 0;

            if (!hasQuestions)
            {
                return 0f;
            }

            var calculatedScore = (float)CorrectCount / Questions.Count;
            return calculatedScore;
        }
    }

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
            Difficulty = command.Difficulty,
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

        var targetQuestion = Questions.FirstOrDefault(q => q.Id == command.QuestionId);

        if (targetQuestion is null)
        {
            return QuizErrors.QuestionNotFound;
        }

        if (targetQuestion.IsAnswered)
        {
            return QuizErrors.AlreadyAnswered;
        }

        var isCorrectAnswer = targetQuestion.CorrectIndex == command.SelectedIndex;

        var events = new List<IEvent>
        {
            new QuizQuestionAnswered
            {
                QuizId = Id,
                UserId = UserId,
                ConceptId = targetQuestion.TopicId,
                QuestionId = command.QuestionId,
                SelectedIndex = command.SelectedIndex,
                IsCorrect = isCorrectAnswer,
                AnsweredAt = command.AnsweredAt
            }
        };

        var isFinalQuestion = (AnsweredCount + 1) == Questions.Count;
        var isAlreadyCompleted = Status == QuizStatus.Completed;

        if (!isAlreadyCompleted && isFinalQuestion)
        {
            var finalCorrectCount = CorrectCount;

            if (isCorrectAnswer)
            {
                finalCorrectCount++;
            }

            var finalIsPerfect = finalCorrectCount == Questions.Count;
            var finalScore = (float)finalCorrectCount / Questions.Count;

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
            DesiredQuestionsCount = null,
            Difficulty = DifficultyLevel.Unspecified,
            Questions = [],
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
            Difficulty = @event.DifficultyLevel,
            Questions = [],
            CreatedAt = @event.CreatedAt,
            CompletedAt = null
        };
    }

    public Quiz Apply(QuizQuestionAnswered @event)
    {
        var updatedQuestions = new List<QuizQuestion>(Questions.Count);

        foreach (var question in Questions)
        {
            var isTargetQuestion = question.Id == @event.QuestionId;

            if (isTargetQuestion)
            {
                var answeredQuestion = question with { SelectedIndex = @event.SelectedIndex };
                updatedQuestions.Add(answeredQuestion);
            }
            else
            {
                var unchangedQuestion = question;
                updatedQuestions.Add(unchangedQuestion);
            }
        }

        return this with
        {
            Status = QuizStatus.InProgress,
            Questions = updatedQuestions
        };
    }

    public Quiz Apply(QuizContentGenerated @event)
    {
        var resolvedDifficulty = Difficulty;
        var isDifficultyUnspecified = Difficulty == DifficultyLevel.Unspecified;

        if (isDifficultyUnspecified)
        {
            resolvedDifficulty = @event.Difficulty;
        }

        return this with
        {
            Questions = @event.Questions.ToList(),
            Difficulty = resolvedDifficulty,
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