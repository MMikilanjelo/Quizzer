using Application.Abstractions.Messaging;
using Application.Authentication;
using Application.Users.Queries;
using Identity.Contracts;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Users;

internal sealed class GetMyDashboardEndpoint : IEndpoint<UserEndpointGroup>
{
    public void MapEndpoint(RouteGroupBuilder group) =>
        group
            .MapGet("me/dashboard", Handler)
            .RequireAuthorization(Policies.RequireOnboardingComplete);

    private static async Task<IResult> Handler(
        IUserContext userContext,
        IQueryHandler<GetUserDashboard.Query, GetUserDashboard.Response> handler,
        CancellationToken cancellationToken
    )
    {
        var query = new GetUserDashboard.Query
        {
            UserId = userContext.UserId
        };

        var result = await handler.Handle(query, cancellationToken);

        return result.Match(
            response => Results.Ok(response.Dashboard),
            CustomResults.Problem
        );
    }
}