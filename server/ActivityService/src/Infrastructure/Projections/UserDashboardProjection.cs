using Application.Quizzes.Views;
using Domain.Learning;
using Domain.Quizzes;
using Marten.Events.Projections;

namespace Infrastructure.Projections;

public class UserDashboardProjection : MultiStreamProjection<UserDashboardView, string>
{
    public UserDashboardProjection()
    {
        Identity<QuizCompleted>(e => e.UserId);
        Identity<TopicMasteryUpdated>(e => e.UserId);
        Identity<QuizQuestionAnswered>(e => e.UserId);
    }

    public UserDashboardView Create(QuizCompleted @event) =>
        new()
        {
            Id = @event.UserId,
            TotalQuizzes = 1,
            PerfectQuizzes = @event.IsPerfect ? 1 : 0,
            AverageScore = @event.ScorePercentage,
            LastQuizDate = @event.CompletedAt,
            StreakDays = 1,
            TopicMasteryLevels = []
        };

    public UserDashboardView Apply(QuizCompleted @event, UserDashboardView current)
    {
        var newStreak = current.StreakDays;
        if (current.LastQuizDate.HasValue)
        {
            var daysSinceLastQuiz = (@event.CompletedAt.Date - current.LastQuizDate.Value.Date).Days;

            switch (daysSinceLastQuiz)
            {
                case 1:
                    newStreak++;
                    break;
                case > 1:
                    newStreak = 1;
                    break;
            }
        }

        var newAverage = (current.AverageScore * current.TotalQuizzes + @event.ScorePercentage) / (current.TotalQuizzes + 1);

        return current with
        {
            TotalQuizzes = current.TotalQuizzes + 1,
            PerfectQuizzes = current.PerfectQuizzes + (@event.IsPerfect ? 1 : 0),
            AverageScore = newAverage,
            StreakDays = newStreak,
            LastQuizDate = @event.CompletedAt
        };
    }

    public UserDashboardView Apply(TopicMasteryUpdated @event, UserDashboardView current)
    {
        var conceptMasteryLevels = current.TopicMasteryLevels.ToList();
        var index = conceptMasteryLevels.FindIndex(m => m.TopicId == @event.TopicId);

        var newPercentage = (int)(@event.NewMastery * 100);

        if (index == -1)
        {
            conceptMasteryLevels.Add(new TopicMasteryLevelView
            {
                TopicId = @event.TopicId,
                MasteryPercentage = newPercentage
            });
        }
        else
        {
            conceptMasteryLevels[index] = conceptMasteryLevels[index] with
            {
                MasteryPercentage = newPercentage
            };
        }

        return current with { TopicMasteryLevels = conceptMasteryLevels };
    }

    public UserDashboardView Apply(QuizQuestionAnswered @event, UserDashboardView current)
    {
        var newTotalQuestions = current.TotalQuestionsAnswered + 1;
        var newTotalCorrect = current.TotalCorrectAnswers + (@event.IsCorrect ? 1 : 0);

        var conceptPerformanceList = current.ConceptPerformance.ToList();
        var index = conceptPerformanceList.FindIndex(c => c.TopicId == @event.TopicId);

        if (index == -1)
        {
            conceptPerformanceList.Add(new ConceptPerformanceView
            {
                TopicId = @event.TopicId,
                TotalAnswered = 1,
                TotalCorrect = @event.IsCorrect ? 1 : 0
            });
        }
        else
        {
            var existing = conceptPerformanceList[index];
            conceptPerformanceList[index] = existing with
            {
                TotalAnswered = existing.TotalAnswered + 1,
                TotalCorrect = existing.TotalCorrect + (@event.IsCorrect ? 1 : 0)
            };
        }

        return current with
        {
            TotalQuestionsAnswered = newTotalQuestions,
            TotalCorrectAnswers = newTotalCorrect,
            ConceptPerformance = conceptPerformanceList
        };
    }
}