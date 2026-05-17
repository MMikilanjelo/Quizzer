using Application.Abstractions.Messaging;
using Application.Abstractions.Providers;
using Application.Authentication;
using Domain.Quizzes;
using ErrorOr;
using FluentValidation;
using Marten;

namespace Application.Quizzes.Commands;

public static class ManualScheduleQuiz
{
    public sealed record Command : ICommand<Response>
    {
        public required string UserId { get; init; }
        public required string DomainId { get; init; }
        public required int QuestionCount { get; set; }
        public required Quiz.DifficultyLevel DifficultyLevel { get; set; }
    }

    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty()
                .WithMessage("User ID is required.");

            RuleFor(x => x.DomainId)
                .NotEmpty()
                .WithMessage("Topic ID is required.");

            RuleFor(x => x.QuestionCount)
                .InclusiveBetween(Quiz.MinQuestionCount, Quiz.MaxQuestionCount)
                .WithMessage($"Question count must be between {Quiz.MinQuestionCount} and {Quiz.MaxQuestionCount}.");

            RuleFor(x => x.DifficultyLevel)
                .IsInEnum()
                .WithMessage("A valid difficulty level must be specified.");
        }
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
                DomainId = command.DomainId,
                QuestionCount = command.QuestionCount,
                DifficultyLevel = command.DifficultyLevel,
                SequenceNumber = userQuizCount + 1,
                CreatedAt = dateTimeProvider.UtcNow,
            };

            documentSession.Events.StartStream<Quiz>(quizId, quizScheduled);

            await documentSession.SaveChangesAsync(cancellationToken);

            return new Response(quizId);
        }
    }
}