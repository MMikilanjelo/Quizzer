using Application.Abstractions;
using Application.Abstractions.Messaging;
using Application.Abstractions.Providers;
using Domain.Learning;
using Domain.Quizzes;
using ErrorOr;
using FluentValidation;
using Marten;

namespace Application.Quizzes.Commands;

public static class SubmitQuizAnswer
{
    public sealed record Command : ICommand
    {
        public required string UserId { get; init; }
        public required string QuizId { get; init; }
        public required string QuestionId { get; init; }
        public int SelectedAnswerIndex { get; init; }
    }

    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty()
                .WithMessage("User ID is required.");

            RuleFor(x => x.QuizId)
                .NotEmpty()
                .WithMessage("Quiz ID is required.");

            RuleFor(x => x.QuestionId)
                .NotEmpty()
                .WithMessage("Question ID is required.");

            RuleFor(x => x.SelectedAnswerIndex)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Selected answer index must be zero or greater.");
        }
    }

    internal sealed class Handler(
        IDocumentSession session,
        IDateTimeProvider dateTimeProvider
    ) : ICommandHandler<Command>
    {
        public async Task<ErrorOr<Success>> HandleAsync(Command command, CancellationToken cancellationToken)
        {
            var stream = await session.Events.FetchForWriting<Quiz>(command.QuizId, cancellationToken);

            var quiz = stream.Aggregate;

            if (quiz is null)
            {
                return QuizErrors.NotFound;
            }

            var question = quiz.Questions.FirstOrDefault(q => q.Id == command.QuestionId);

            if (question is null)
            {
                return QuizErrors.QuestionNotFound;
            }

            var answerQuestionCommand = new AnswerQuestionCommand
            {
                QuestionId = command.QuestionId,
                SelectedIndex = command.SelectedAnswerIndex,
                AttemptingUserId = command.UserId,
                AnsweredAt = dateTimeProvider.UtcNow
            };

            var answerResult = quiz.AnswerQuestion(answerQuestionCommand);

            if (answerResult.IsError)
            {
                return answerResult.Errors;
            }

            stream.AppendMany(answerResult.Value);

            var answeredEvent = answerResult.Value
                .OfType<QuizQuestionAnswered>()
                .First();

            var masteryStreamId = ConceptMastery.FormatId(
                answeredEvent.UserId,
                answeredEvent.ConceptId);

            var masteryStream = await session.Events
                .FetchForWriting<ConceptMastery>(
                    masteryStreamId,
                    cancellationToken);

            var mastery = masteryStream.Aggregate;

            if (mastery is null)
            {
                var started = new ConceptMasteryStarted(
                    answeredEvent.UserId,
                    answeredEvent.ConceptId,
                    BktParams.Initial,
                    dateTimeProvider.UtcNow
                );

                masteryStream.AppendOne(started);

                mastery = ConceptMastery.Create(started);
            }

            var updated = mastery.RecordAttempt(
                answeredEvent.IsCorrect,
                answeredEvent.QuizId,
                answeredEvent.QuestionId,
                dateTimeProvider.UtcNow
            );

            masteryStream.AppendOne(updated);

            await session.SaveChangesAsync(cancellationToken);

            return Result.Success;
        }
    }
}