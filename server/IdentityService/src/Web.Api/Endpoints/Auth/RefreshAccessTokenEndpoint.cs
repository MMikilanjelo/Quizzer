using Application.Abstractions.Messaging;
using Application.Users.Commands;
using Web.Api.Endpoints.Users;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Auth;

internal sealed class RefreshAccessTokenEndpoint : IEndpoint<UserEndpointGroup>
{
    private sealed record Request(string RefreshToken);

    public void MapEndpoint(RouteGroupBuilder group) =>
        group.MapPost("refresh", Handler).AllowAnonymous();

    private static async Task<IResult> Handler(
        Request request,
        ICommandHandler<RefreshAccessToken.Command, RefreshAccessToken.Response> handler,
        CancellationToken cancellationToken
    )
    {
        var command = new RefreshAccessToken.Command
        {
            RefreshToken = request.RefreshToken
        };

        var result = await handler.HandleAsync(command, cancellationToken);

        return result.Match(Results.Ok, CustomResults.Problem);
    }
}