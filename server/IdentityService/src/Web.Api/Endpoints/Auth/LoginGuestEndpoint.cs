using Application.Abstractions.Messaging;
using Application.Users.Commands;
using Web.Api.Endpoints.Users;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Auth;

internal sealed class LoginGuestEndpoint : IEndpoint<UserEndpointGroup>
{
    private sealed record Request(string GuestId);

    public void MapEndpoint(RouteGroupBuilder group) =>
        group.MapPost("guests/login", Handler).AllowAnonymous();

    private static async Task<IResult> Handler(
        Request request,
        ICommandHandler<LoginGuest.Command, LoginGuest.Response> handler,
        CancellationToken cancellationToken
    )
    {
        var command = new LoginGuest.Command
        {
            GuestId = request.GuestId
        };

        var result = await handler.HandleAsync(command, cancellationToken);

        return result.Match(Results.Ok, CustomResults.Problem);
    }
}