using Application.Abstractions.Messaging;
using Application.Abstractions.Providers;
using Domain.Users;
using ErrorOr;
using FluentValidation;
using Marten;

namespace Application.Users.Commands;

public static class RegisterGuest
{
    public sealed record Command : ICommand
    {
        public required string GuestId { get; init; }
    }

    internal sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.GuestId)
                .NotEmpty()
                .NotNull();
        }
    }

    internal sealed class Handler(
        IDateTimeProvider timeProvider,
        IDocumentSession session
    ) : ICommandHandler<Command>
    {
        public async Task<ErrorOr<Success>> HandleAsync(Command command, CancellationToken cancellationToken)
        {
            var user = await session.Query<User>()
                .Where(x => x.GuestId == command.GuestId)
                .FirstOrDefaultAsync(cancellationToken);

            if (user is not null)
            {
                return UserErrors.DeviceAlreadyLinked;
            }

            var userRegisteredEvent = User.Decide(new RegisterCommand(command.GuestId, timeProvider.UtcNow));

            session.Events.StartStream<User>(
                userRegisteredEvent.UserId,
                userRegisteredEvent
            );

            await session.SaveChangesAsync(cancellationToken);

            return Result.Success;
        }
    }
}