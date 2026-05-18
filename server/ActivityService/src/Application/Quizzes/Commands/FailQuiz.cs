using Application.Abstractions.Messaging;
using Application.Abstractions.Providers;
using Domain.Quizzes;
using ErrorOr;
using Marten;
using Microsoft.Extensions.Logging;

namespace Application.Quizzes.Commands;

public static class FailQuiz
{
    public sealed record Command : ICommand
    {
        public required string QuizId { get; init; }
    }

    internal sealed class Handler(
        IDocumentSession documentSession,
        IDateTimeProvider dateTimeProvider
    ) : ICommandHandler<Command>
    {
        public async Task<ErrorOr<Success>> HandleAsync(Command command, CancellationToken cancellationToken)
        {
            var stream = await documentSession.Events.FetchForWriting<Quiz>(command.QuizId, cancellationToken);
            var quiz = stream.Aggregate;

            if (quiz is null)
            {
                return QuizErrors.NotFound;
            }

            var failedEventResult = quiz.Fail(dateTimeProvider.UtcNow);

            if (failedEventResult.IsError)
            {
                return failedEventResult.Errors;
            }

            stream.AppendOne(failedEventResult.Value);

            await documentSession.SaveChangesAsync(cancellationToken);

            return Result.Success;
        }
    }
}