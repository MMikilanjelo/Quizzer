using Application.Abstractions.Messaging;
using Application.Authentication;
using Application.Users;
using Application.Users.Commands;
using Domain.Users;
using ErrorOr;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Users;

internal sealed class SubmitOnboarding : IEndpoint<UserEndpointGroup>
{
    private sealed record Request(
        List<Goal> Goals,
        List<Interest> Interests,
        Proficiency Proficiency
    );

    public void MapEndpoint(RouteGroupBuilder group) =>
        group.MapPost("me/onboarding", Handler).RequireAuthorization();

    private static async Task<IResult> Handler(
        Request request,
        ICommandHandler<Application.Users.Commands.SubmitOnboarding.Command> handler,
        IUserContext userContext,
        CancellationToken ct
    )
    {
        var command = new Application.Users.Commands.SubmitOnboarding.Command
        {
            UserId = userContext.UserId,
            Goals = request.Goals,
            Interests = request.Interests,
            Proficiency = request.Proficiency
        };

        var result = await handler.HandleAsync(command, ct);

        return result.Match(
            _ => Results.NoContent(),
            CustomResults.Problem
        );
    }
}