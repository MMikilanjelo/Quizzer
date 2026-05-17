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
            var userQuizCount = await documentSession.Query<Quiz>()
                .CountAsync(q => q.UserId == command.UserId, cancellationToken);

            var quizId = Guid.NewGuid().ToString();

            var quizScheduled = new ManualQuizScheduled
            {
                QuizId = quizId,
                UserId = command.UserId,
                TopicId = command.TopicId,
                QuestionCount = command.QuestionCount,
                DifficultyLevelId = command.DifficultyLevelId,
                SequenceNumber = userQuizCount + 1,
                CreatedAt = dateTimeProvider.UtcNow,
            };

            documentSession.Events.StartStream<Quiz>(quizId, quizScheduled);

            await documentSession.SaveChangesAsync(cancellationToken);

            return new Response(quizId);
        }
    }
}