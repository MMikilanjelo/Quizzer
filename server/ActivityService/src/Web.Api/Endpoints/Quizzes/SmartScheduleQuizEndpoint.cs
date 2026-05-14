using Application.Abstractions.Messaging;
using Application.Authentication;
using Application.Quizzes;
using Application.Quizzes.Commands;
using ErrorOr;
using Identity.Contracts;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Quizzes;

internal sealed class SmartScheduleQuizEndpoint : IEndpoint<QuizEndpointGroup>
{
    private sealed record Request(string TopicId);

    public void MapEndpoint(RouteGroupBuilder group) =>
        group
            .MapPost("schedule/smart", Handler)
            .RequireAuthorization(Policies.RequireOnboardingComplete);

    private static async Task<IResult> Handler(
        Request request,
        ICommandHandler<SmartScheduleQuiz.Command, SmartScheduleQuiz.Response> handler,
        IUserContext userContext,
        CancellationToken cancellationToken
    )
    {
        var command = new SmartScheduleQuiz.Command
        {
            UserId = userContext.UserId
        };

        ErrorOr<SmartScheduleQuiz.Response> result = await handler.HandleAsync(command, cancellationToken);

        return result.Match(
            response => Results.Accepted($"quizzes/{response.Id}", response),
            CustomResults.Problem
        );
    }
}