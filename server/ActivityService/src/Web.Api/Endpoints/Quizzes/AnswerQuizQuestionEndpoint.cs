using Application.Abstractions.Messaging;
using Application.Authentication;
using Application.Quizzes;
using Application.Quizzes.Commands;
using ErrorOr;
using Identity.Contracts;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Quizzes;

internal class AnswerQuizQuestionEndpoint : IEndpoint<QuizEndpointGroup>
{
    private sealed record Request(string QuestionId, int SelectedIndex);

    public void MapEndpoint(RouteGroupBuilder group) =>
        group
            .MapPost("{quizId}/answer", Handler)
            .RequireAuthorization(Policies.RequireOnboardingComplete);

    private static async Task<IResult> Handler(
        string quizId,
        Request request,
        ICommandHandler<SubmitQuizAnswer.Command> handler,
        IUserContext userContext,
        CancellationToken cancellationToken
    )
    {
        var command = new SubmitQuizAnswer.Command
        {
            UserId = userContext.UserId,
            QuestionId = request.QuestionId,
            QuizId = quizId,
            SelectedAnswerIndex = request.SelectedIndex
        };

        ErrorOr<Success> result = await handler.HandleAsync(command, cancellationToken);

        return result.Match(
            _ => Results.Ok(),
            CustomResults.Problem
        );
    }
}