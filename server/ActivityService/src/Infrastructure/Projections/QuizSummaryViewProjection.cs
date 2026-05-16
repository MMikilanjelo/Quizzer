using Application.Quizzes.Views;
using Domain.Learning;
using Domain.Quizzes;
using Marten.Events.Aggregation;
using Marten.Events.Projections;

namespace Infrastructure.Projections;

public class QuizSummaryViewProjection : MultiStreamProjection<QuizSummaryView, string>
{
    public QuizSummaryViewProjection()
    {
        Identity<QuizScheduled>(e => e.QuizId);
        Identity<QuizContentGenerated>(e => e.QuizId);
        Identity<QuizQuestionAnswered>(e => e.QuizId);
        Identity<QuizCompleted>(e => e.QuizId);
        Identity<ConceptMasteryUpdated>(e => e.QuizId);
    }

    public QuizSummaryView Create(QuizScheduled @event) =>
        new()
        {
            Id = @event.QuizId,
            Name = $"Quiz #{@event.SequenceNumber}",
            UserId = @event.UserId,
            Topics = [],
            Status = QuizStatus.Pending,
            CreatedAt = @event.CreatedAt,
            QuestionCount = 0,
            AnsweredCount = 0,
            Questions = [],
            MasteryChanges = []
        };

    public QuizSummaryView Apply(QuizContentGenerated @event, QuizSummaryView current) =>
        current with
        {
            Status = QuizStatus.Ready,
            Topics = @event.Questions.Select(q => q.ConceptId).Distinct().ToList(),
            QuestionCount = @event.Questions.Count,
            Questions = @event.Questions.Select(q => new QuestionView
            {
                Id = q.Id,
                ConceptId = q.ConceptId,
                Text = q.Text,
                Options = q.Options.ToList(),
                CorrectAnswerIndex = q.CorrectIndex,
                SelectedAnswerIndex = null
            }).ToList()
        };

    public QuizSummaryView Apply(QuizQuestionAnswered @event, QuizSummaryView current)
    {
        var updatedQuestions = current.Questions.Select(q =>
        {
            if (q.Id == @event.QuestionId)
            {
                return q with { SelectedAnswerIndex = @event.SelectedIndex };
            }

            return q;
        }).ToList();

        return current with
        {
            Status = QuizStatus.InProgress,
            AnsweredCount = current.AnsweredCount + 1,
            Questions = updatedQuestions
        };
    }

    public QuizSummaryView Apply(QuizCompleted @event, QuizSummaryView current) =>
        current with
        {
            Status = QuizStatus.Completed
        };

    public QuizSummaryView Apply(ConceptMasteryUpdated @event, QuizSummaryView current)
    {
        var updatedChanges = current.MasteryChanges.ToList();

        var index = updatedChanges.FindIndex(m => m.ConceptId == @event.ConceptId);

        if (index == -1)
        {
            updatedChanges.Add(new ConceptMasteryDeltaView
            {
                ConceptId = @event.ConceptId,
                StartingMastery = @event.OldMastery,
                EndingMastery = @event.NewMastery,
                AttemptsDuringQuiz = 1
            });
        }
        else
        {
            var existing = updatedChanges[index];

            updatedChanges[index] = existing with
            {
                EndingMastery = @event.NewMastery,
                AttemptsDuringQuiz = existing.AttemptsDuringQuiz + 1
            };
        }

        return current with { MasteryChanges = updatedChanges };
    }
}