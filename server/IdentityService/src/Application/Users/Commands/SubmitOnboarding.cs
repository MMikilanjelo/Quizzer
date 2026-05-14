using Application.Abstractions.Messaging;
using Application.Abstractions.Providers;
using Domain.Users;
using ErrorOr;
using FluentValidation;
using Marten;

namespace Application.Users.Commands;

public static class SubmitOnboarding
{
    public sealed record Command : ICommand
    {
        public required string UserId { get; init; }
        public required List<Goal> Goals { get; init; }
        public required List<Interest> Interests { get; init; }
        public required Proficiency Proficiency { get; init; }
    }

    internal sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty()
                .WithMessage("User ID is required.");

            RuleFor(x => x.Goals)
                .NotEmpty()
                .WithMessage("At least one Goal must be selected.");

            RuleForEach(x => x.Goals).IsInEnum();

            RuleFor(x => x.Interests)
                .NotEmpty()
                .WithMessage("At least one Interest must be selected.");

            RuleForEach(x => x.Interests).IsInEnum();

            RuleFor(x => x.Proficiency).IsInEnum();
        }
    }

    internal sealed class Handler(
        IDateTimeProvider timeProvider,
        IDocumentSession session
    ) : ICommandHandler<Command>
    {
        public async Task<ErrorOr<Success>> HandleAsync(Command command, CancellationToken ct)
        {
            var stream = await session.Events.FetchForWriting<User>(
                command.UserId,
                cancellation: ct
            );

            if (stream.Aggregate is null)
            {
                return UserErrors.NotFound;
            }

            var result = stream.Aggregate.Decide(
                new CompleteOnboardingCommand(
                    command.Goals,
                    command.Interests,
                    command.Proficiency,
                    timeProvider.UtcNow
                )
            );

            if (result.IsError)
            {
                return result.Errors;
            }

            stream.AppendOne(result.Value);

            await session.SaveChangesAsync(ct);

            return Result.Success;
        }
    }
}