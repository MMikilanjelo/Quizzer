using Application.Abstractions.Messaging;
using Application.Authentication;
using Application.Users.Queries;
using Identity.Contracts;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Users;

internal sealed class GetUserProfileEndpoint : IEndpoint<UserEndpointGroup>
{
    public void MapEndpoint(RouteGroupBuilder group) =>
        group
            .MapGet("me/profile", Handler)
            .RequireAuthorization(Policies.RequireOnboardingComplete);

    private static async Task<IResult> Handler(
        IUserContext userContext,
        IQueryHandler<GetUserProfile.Query, GetUserProfile.Response> handler,
        CancellationToken cancellationToken
    )
    {
        var query = new GetUserProfile.Query(userContext.UserId);

        var result = await handler.Handle(query, cancellationToken);

        return result.Match(Results.Ok, CustomResults.Problem);
    }
}