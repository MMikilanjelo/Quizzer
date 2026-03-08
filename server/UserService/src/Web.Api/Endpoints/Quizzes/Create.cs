using Application.Abstractions.Messaging;
using Application.Quizzes.Create;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Quizzes;

internal sealed class Create : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/quizzes").WithTags(Tags.Quizzes);

        group.MapPost(string.Empty, Handler);
    }

    private async Task<IResult> Handler(ICommandHandler<CreateQuizCommand, Guid> handler, CancellationToken cancellationToken)
    {
        var command = new CreateQuizCommand();

        var result = await handler.Handle(command, cancellationToken);

        return result.Match(Results.Ok, CustomResults.Problem);
    }
}