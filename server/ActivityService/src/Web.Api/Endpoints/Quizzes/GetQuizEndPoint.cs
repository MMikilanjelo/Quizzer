using Application.Abstractions.Messaging;
using Application.Quizzes.Queries;
using Identity.Contracts;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Quizzes;

internal sealed class GetQuizEndpoint : IEndpoint<QuizEndpointGroup>
{
    public void MapEndpoint(RouteGroupBuilder group) =>
        group
            .MapGet("{quizId}", Handler)
            .RequireAuthorization(Policies.RequireOnboardingComplete);

    private static async Task<IResult> Handler(
        string quizId,
        IQueryHandler<GetQuiz.Query, GetQuiz.Response> handler,
        CancellationToken cancellationToken
    )
    {
        var query = new GetQuiz.Query
        {
            QuizId = quizId
        };

        var result = await handler.Handle(query, cancellationToken);

        return result.Match(
            Results.Ok,
            CustomResults.Problem
        );
    }
}