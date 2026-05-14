using Application.Abstractions.Messaging;
using Application.Authentication;
using Application.Quizzes.Commands;
using ErrorOr;
using Identity.Contracts;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Quizzes;

internal sealed class ManualScheduleQuizEndpoint : IEndpoint<QuizEndpointGroup>
{
    private sealed record Request
    {
        public required string TopicId { get; set; }
        public required int QuestionCount { get; set; }
        public required string DifficultyLevelId { get; set; }
    }

    public void MapEndpoint(RouteGroupBuilder group) =>
        group
            .MapPost("schedule/manual", Handler)
            .RequireAuthorization(Policies.RequireOnboardingComplete);

    private static async Task<IResult> Handler(
        Request request,
        ICommandHandler<ManualScheduleQuiz.Command, ManualScheduleQuiz.Response> handler,
        IUserContext userContext,
        CancellationToken cancellationToken
    )
    {
        var command = new ManualScheduleQuiz.Command
        {
            TopicId = request.TopicId,
            UserId = userContext.UserId,
            QuestionCount = request.QuestionCount,
            DifficultyLevelId = request.DifficultyLevelId
        };

        var result = await handler.HandleAsync(command, cancellationToken);

        return result.Match(
            response => Results.Accepted($"quizzes/{response.Id}", response),
            CustomResults.Problem
        );
    }
}