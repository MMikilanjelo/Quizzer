using Application.Abstractions.Messaging;
using Application.Mapping;
using Application.Quizzes.Views;
using ErrorOr;
using Marten;
using Marten.Linq;
using Marten.Pagination;

namespace Application.Quizzes.Queries;

public static class GetUserQuizzes
{
    public enum QuizFilter
    {
        All = 0,
        Pending = 1,
        Active = 2,
        Completed = 3
    }

    public sealed record Query : IQuery<Response>
    {
        public required string UserId { get; init; }
        public required QuizFilter Filter { get; init; }
        public required int Page { get; init; }
        public required int PageSize { get; init; }
    }

    public sealed record Response
    {
        public required Page<Model> Quizzes { get; init; }
    }

    public sealed record Model
    {
        public required string Id { get; set; }
        public required string UserId { get; set; }
        public required List<string> Topics { get; set; }
        public required string Name { get; set; }
        public required QuizStatus Status { get; set; }
        public int QuestionCount { get; init; }
        public int AnsweredCount { get; init; }
        public DateTime CreatedAt { get; set; }
    }

    internal sealed class Handler(IQuerySession session) : IQueryHandler<Query, Response>
    {
        public async Task<ErrorOr<Response>> Handle(Query query, CancellationToken cancellationToken)
        {
            var queryable = session
                .Query<QuizSummaryView>()
                .Where(x => x.UserId == query.UserId);

            queryable = query.Filter switch
            {
                QuizFilter.Pending => queryable.Where(x => x.Status == QuizStatus.Pending),
                QuizFilter.Active => queryable.Where(x => x.Status == QuizStatus.Ready || x.Status == QuizStatus.InProgress),
                QuizFilter.Completed => queryable.Where(x => x.Status == QuizStatus.Completed),
                _ => queryable
            };

            var pagedItems = await queryable
                .OrderByDescending(x => x.CreatedAt)
                .Select(view => new Model
                {
                    Id = view.Id,
                    UserId = view.UserId,
                    Topics = view.Topics,
                    Name = view.Name,
                    Status = view.Status,
                    QuestionCount = view.QuestionCount,
                    AnsweredCount = view.AnsweredCount,
                    CreatedAt = view.CreatedAt
                })
                .ToPagedListAsync(query.Page, query.PageSize, cancellationToken);

            return new Response
            {
                Quizzes = pagedItems.ToPagedResponse()
            };
        }
    }
}