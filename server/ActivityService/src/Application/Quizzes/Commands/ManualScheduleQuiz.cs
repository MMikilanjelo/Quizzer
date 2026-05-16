using Application.Abstractions.Messaging;
using Application.Abstractions.Providers;
using Application.Authentication;
using Domain.Quizzes;
using ErrorOr;
using Marten;

namespace Application.Quizzes.Commands;

public static class ManualScheduleQuiz
{
    public sealed record Command : ICommand<Response>
    {
        public required string UserId { get; init; }
        public required string TopicId { get; init; }
        public required int QuestionCount { get; set; }
        public required string DifficultyLevelId { get; set; }
    }

    public sealed record Response(string Id);

    internal sealed class Handler(
        IDocumentSession documentSession,
        IDateTimeProvider dateTimeProvider
    ) : ICommandHandler<Command, Response>
    {
        public async Task<ErrorOr<Response>> HandleAsync(Command command, CancellationToken cancellationToken)
        {
            var userQuizCount = await documentSession.Query<Quiz>().CountAsync(q => q.UserId == command.UserId, cancellationToken);

            var nextSequence = userQuizCount + 1;

            var quizScheduled = new QuizScheduled
            {
                QuizId = Guid.NewGuid().ToString(),
                UserId = command.UserId,
                Topic = command.TopicId,
                SequenceNumber = nextSequence,
                CreatedAt = dateTimeProvider.UtcNow,
            };

            documentSession.Events.StartStream<Quiz>(quizScheduled.QuizId, quizScheduled);

            await documentSession.SaveChangesAsync(cancellationToken);

            return new Response(quizScheduled.QuizId);
        }
    }
}