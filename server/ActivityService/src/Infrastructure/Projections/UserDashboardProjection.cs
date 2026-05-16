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
        Identity<ConceptMasteryUpdated>(e => e.UserId);
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
            ConceptMasteryLevels = []
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

    public UserDashboardView Apply(ConceptMasteryUpdated @event, UserDashboardView current)
    {
        var conceptMasteryLevels = current.ConceptMasteryLevels.ToList();
        var index = conceptMasteryLevels.FindIndex(m => m.ConceptId == @event.ConceptId);

        var newPercentage = (int)(@event.NewMastery * 100);

        if (index == -1)
        {
            conceptMasteryLevels.Add(new ConceptMasteryLevelView
            {
                ConceptId = @event.ConceptId,
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

        return current with { ConceptMasteryLevels = conceptMasteryLevels };
    }
}