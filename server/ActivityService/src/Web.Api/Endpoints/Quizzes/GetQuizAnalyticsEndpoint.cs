using Application.Abstractions.Messaging;
using Application.Quizzes.Queries;
using Identity.Contracts;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Quizzes;

internal class GetQuizAnalyticsEndpoint : IEndpoint<QuizEndpointGroup>
{
    public void MapEndpoint(RouteGroupBuilder group) =>
        group
            .MapGet("{quizId}/analytics", Handler)
            .RequireAuthorization(Policies.RequireOnboardingComplete);

    private static async Task<IResult> Handler(
        string quizId,
        IQueryHandler<GetQuizAnalytics.Query, GetQuizAnalytics.Response> handler,
        CancellationToken cancellationToken
    )
    {
        var query = new GetQuizAnalytics.Query
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