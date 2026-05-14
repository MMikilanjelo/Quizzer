using Application.Abstractions;
using Application.Abstractions.Messaging;
using Application.Mapping;
using Application.Quizzes.Views;
using ErrorOr;
using Marten;
using Marten.Pagination;

namespace Application.Quizzes.Queries;

public class GetQuizConfiguration
{
    public sealed record Query : IQuery<Response>;

    public sealed record Response
    {
        public int MinQuestions { get; private init; } = 5;

        public int MaxQuestions { get; private init; } = 50;

        public List<string> Difficulties { get; private init; } = ["Easy", "Medium", "Hard"];
        public required List<string> Domains { get; init; }
    }

    internal sealed class Handler(IKnowledgeGraphClient knowledgeGraphClient) : IQueryHandler<Query, Response>
    {
        public async Task<ErrorOr<Response>> Handle(Query query, CancellationToken cancellationToken)
        {
            var domains = await knowledgeGraphClient.GetAvailableDomainsAsync(cancellationToken);

            return new Response
            {
                Domains = domains
            };
        }
    }
}