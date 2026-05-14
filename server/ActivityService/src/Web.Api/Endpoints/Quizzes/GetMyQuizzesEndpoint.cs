using System.Security.Claims;
using Application.Abstractions.Messaging;
using Application.Authentication;
using Application.Quizzes.Queries;
using Identity.Contracts;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Quizzes;

internal sealed class GetMyQuizzesEndpoint : IEndpoint<QuizEndpointGroup>
{
    public void MapEndpoint(RouteGroupBuilder group) =>
        group
            .MapGet("me", Handler)
            .RequireAuthorization(Policies.RequireOnboardingComplete);

    private sealed record QueryParameters
    {
        public GetUserQuizzes.QuizFilter? Filter { get; init; }
        public int? Page { get; init; }
        public int? PageSize { get; init; }
    }

    private static async Task<IResult> Handler(
        [AsParameters] QueryParameters request,
        IUserContext userContext,
        IQueryHandler<GetUserQuizzes.Query, GetUserQuizzes.Response> handler,
        CancellationToken cancellationToken
    )
    {
        var query = new GetUserQuizzes.Query
        {
            UserId = userContext.UserId,
            Filter = request.Filter ?? GetUserQuizzes.QuizFilter.All,
            Page = request.Page ?? 1,
            PageSize = Math.Min(request.PageSize ?? 10, 10)
        };

        var result = await handler.Handle(query, cancellationToken);

        return result.Match(
            Results.Ok,
            CustomResults.Problem
        );
    }
}