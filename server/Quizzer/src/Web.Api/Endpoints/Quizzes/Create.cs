using Application.Abstractions.Messaging;
using Application.Quizzes.Create;
using ErrorOr;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Quizzes;

internal sealed class Create : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("todos", async (
                ICommandHandler<CreateQuizCommand, Guid> handler,
                CancellationToken cancellationToken) =>
            {
                var command = new CreateQuizCommand
                {
                };

                var result = await handler.Handle(command, cancellationToken);

                return result.Match(Results.Ok, CustomResults.Problem);
            })
            .WithTags(Tags.Quizzes);
    }
}