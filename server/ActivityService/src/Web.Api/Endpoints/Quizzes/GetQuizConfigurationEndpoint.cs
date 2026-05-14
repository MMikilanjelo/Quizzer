using Application.Abstractions.Messaging;
using Application.Quizzes.Queries;
using Identity.Contracts;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Quizzes;

internal sealed class GetQuizConfigurationEndpoint : IEndpoint<QuizEndpointGroup>
{
    public void MapEndpoint(RouteGroupBuilder group) =>
        group
            .MapGet("configuration", Handler)
            .RequireAuthorization(Policies.RequireOnboardingComplete);

    private static async Task<IResult> Handler(
        IQueryHandler<GetQuizConfiguration.Query, GetQuizConfiguration.Response> handler,
        CancellationToken cancellationToken
    )
    {
        var query = new GetQuizConfiguration.Query();

        var result = await handler.Handle(query, cancellationToken);

        return result.Match(
            Results.Ok,
            CustomResults.Problem
        );
    }
}