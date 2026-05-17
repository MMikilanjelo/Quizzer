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
        Identity<SmartQuizScheduled>(e => e.QuizId);
        Identity<ManualQuizScheduled>(e => e.QuizId);
        Identity<QuizContentGenerated>(e => e.QuizId);
        Identity<QuizQuestionAnswered>(e => e.QuizId);
        Identity<QuizCompleted>(e => e.QuizId);
        Identity<TopicMasteryUpdated>(e => e.QuizId);
    }

    public QuizSummaryView Create(SmartQuizScheduled @event) =>
        new()
        {
            Id = @event.QuizId,
            Name = $"Quiz #{@event.SequenceNumber}",
            UserId = @event.UserId,
            Topics = [], 
            Status = QuizSummaryView.QuizStatus.Pending,
            CreatedAt = @event.CreatedAt,
            QuestionCount = 0,
            AnsweredCount = 0,
            Questions = [],
            MasteryChanges = []
        };

    public QuizSummaryView Create(ManualQuizScheduled @event) =>
        new()
        {
            Id = @event.QuizId,
            Name = $"Quiz #{@event.SequenceNumber}",
            UserId = @event.UserId,
            Topics = [], 
            Status = QuizSummaryView.QuizStatus.Pending,
            CreatedAt = @event.CreatedAt,
            QuestionCount = @event.QuestionCount, 
            AnsweredCount = 0,
            Questions = [],
            MasteryChanges = []
        };


    public QuizSummaryView Apply(QuizContentGenerated @event, QuizSummaryView current) =>
        current with
        {
            Status = QuizSummaryView.QuizStatus.Ready,
            Topics = @event.Questions.Select(q => q.TopicId).Distinct().ToList(),
            QuestionCount = @event.Questions.Count,
            Questions = @event.Questions.Select(q => new QuestionView
            {
                Id = q.Id,
                TopicId = q.TopicId,
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
            Status = QuizSummaryView.QuizStatus.InProgress,
            AnsweredCount = current.AnsweredCount + 1,
            Questions = updatedQuestions
        };
    }

    public QuizSummaryView Apply(QuizCompleted @event, QuizSummaryView current) =>
        current with { Status = QuizSummaryView.QuizStatus.Completed };

    public QuizSummaryView Apply(TopicMasteryUpdated @event, QuizSummaryView current)
    {
        var updatedChanges = current.MasteryChanges.ToList();

        var index = updatedChanges.FindIndex(m => m.TopicId == @event.TopicId);

        if (index == -1)
        {
            updatedChanges.Add(new TopicMasteryDeltaView
            {
                TopicId = @event.TopicId,
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