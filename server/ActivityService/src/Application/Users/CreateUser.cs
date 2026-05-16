using Application.Abstractions.Messaging;
using Domain.Users;
using ErrorOr;
using Marten;

namespace Application.Users;

public static class CreateUser
{
    public sealed record Command : ICommand
    {
        public required string UserId { get; init; }
        public required List<string> Goals { get; init; }
        public required List<string> Interests { get; init; }
        public required string Proficiency { get; init; }
        public required DateTime OnboardedAt { get; init; }
    }

    internal sealed class Handler(
        IDocumentSession documentSession
    ) : ICommandHandler<Command>
    {
        public async Task<ErrorOr<Success>> HandleAsync(Command command, CancellationToken cancellationToken)
        {
            var user = new User
            {
                Id = command.UserId,
                Goals = command.Goals.Select(Enum.Parse<Goal>).ToList(),
                Interests = command.Interests.Select(Enum.Parse<Interest>).ToList(),
                Proficiency = Enum.Parse<Proficiency>(command.Proficiency),
                OnboardedAt = command.OnboardedAt
            };

            documentSession.Store(user);

            await documentSession.SaveChangesAsync(cancellationToken);

            return Result.Success;
        }
    }
}