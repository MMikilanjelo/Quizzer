using Application.Abstractions;
using Application.Abstractions.Messaging;
using Application.Abstractions.Providers;
using Application.Authentication;
using Domain.Quizzes;
using ErrorOr;
using Marten;

namespace Application.Quizzes.Commands;

public static class SmartScheduleQuiz
{
    public sealed record Command : ICommand<Response>
    {
        public required string UserId { get; init; }
    }

    public sealed record Response(string Id);

    internal sealed class Handler(
        IDocumentSession documentSession,
        IDateTimeProvider dateTimeProvider,
        IKnowledgeGraphClient graphClient
    ) : ICommandHandler<Command, Response>
    {
        public async Task<ErrorOr<Response>> HandleAsync(Command command, CancellationToken cancellationToken)
        {
            var priorityDomains = await graphClient.GetGlobalPriorityDomainsAsync(command.UserId, limit: 1, cancellationToken);

            var assignedDomainId = priorityDomains.First();

            var userQuizCount = await documentSession.Query<Quiz>().CountAsync(q => q.UserId == command.UserId, cancellationToken);

            var quizId = Guid.NewGuid().ToString();

            var quizScheduled = new SmartQuizScheduled
            {
                QuizId = quizId,
                UserId = command.UserId,
                DomainId = assignedDomainId,
                SequenceNumber = userQuizCount + 1,
                CreatedAt = dateTimeProvider.UtcNow,
            };

            documentSession.Events.StartStream<Quiz>(quizScheduled.QuizId, quizScheduled);

            await documentSession.SaveChangesAsync(cancellationToken);

            return new Response(quizScheduled.QuizId);
        }
    }
}