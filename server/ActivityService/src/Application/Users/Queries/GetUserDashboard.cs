using Application.Abstractions.Messaging;
using Application.Quizzes.Views;
using ErrorOr;
using Marten;

namespace Application.Users.Queries;

public static class GetUserDashboard
{
    public sealed record Query : IQuery<Response>
    {
        public required string UserId { get; init; }
    }

    public sealed record Response
    {
        public required DashboardModel Dashboard { get; init; }
    }

    public sealed record DashboardModel
    {
        public required string Id { get; init; }
        public required float AverageScore { get; init; }
        public required int TotalQuizzes { get; init; }
        public required int PerfectQuizzes { get; init; }
        public required int StreakDays { get; init; }
        public DateTime? LastQuizDate { get; init; }
        public required List<TopicMasteryModel> TopStrengths { get; init; }
        public required List<TopicMasteryModel> FocusAreas { get; init; }
    }

    public sealed record TopicMasteryModel
    {
        public required string TopicId { get; init; }
        public required int MasteryPercentage { get; init; }
    }

    internal sealed class Handler(IQuerySession session) : IQueryHandler<Query, Response>
    {
        public async Task<ErrorOr<Response>> Handle(Query query, CancellationToken cancellationToken)
        {
            var view = await session.LoadAsync<UserDashboardView>(query.UserId, cancellationToken) ?? new UserDashboardView
            {
                Id = query.UserId,
                AverageScore = 0,
                TotalQuizzes = 0,
                PerfectQuizzes = 0,
                StreakDays = 0,
                LastQuizDate = null,
                TopicMasteryLevels = []
            };

            var dashboardModel = new DashboardModel
            {
                Id = view.Id,
                AverageScore = view.AverageScore,
                TotalQuizzes = view.TotalQuizzes,
                PerfectQuizzes = view.PerfectQuizzes,
                StreakDays = view.StreakDays,
                LastQuizDate = view.LastQuizDate,

                TopStrengths = view.TopStrengths.Select(x => new TopicMasteryModel
                {
                    TopicId = x.TopicId,
                    MasteryPercentage = x.MasteryPercentage
                }).ToList(),

                FocusAreas = view.FocusAreas.Select(x => new TopicMasteryModel
                {
                    TopicId = x.TopicId,
                    MasteryPercentage = x.MasteryPercentage
                }).ToList()
            };

            return new Response
            {
                Dashboard = dashboardModel
            };
        }
    }
}