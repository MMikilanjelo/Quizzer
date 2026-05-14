using Application.Abstractions.Messaging;
using Application.Users;
using Application.Users.Commands;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Users;

internal sealed class RegisterGuest : IEndpoint<UserEndpointGroup>
{
    private sealed record Request(string GuestId);

    public void MapEndpoint(RouteGroupBuilder group) =>
        group
            .MapPost("guests", Handler)
            .AllowAnonymous();

    private static async Task<IResult> Handler(
        Request request,
        ICommandHandler<Application.Users.Commands.RegisterGuest.Command> handler,
        CancellationToken cancellationToken
    )
    {
        var command = new Application.Users.Commands.RegisterGuest.Command
        {
            GuestId = request.GuestId
        };

        var result = await handler.HandleAsync(command, cancellationToken);

        return result.Match(Results.Ok, CustomResults.Problem);
    }
}