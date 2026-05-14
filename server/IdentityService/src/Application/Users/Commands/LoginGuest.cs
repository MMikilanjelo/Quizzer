using Application.Abstractions.Messaging;
using Application.Abstractions.Providers;
using Application.Authentication;
using Domain.Sessions;
using Domain.Users;
using ErrorOr;
using FluentValidation;
using Marten;

namespace Application.Users.Commands;

public static class LoginGuest
{
    public record Command : ICommand<Response>
    {
        public required string GuestId { get; init; }
    }

    public record Response
    {
        public required string AccessToken { get; init; }
        public required string RefreshToken { get; init; }
        public required List<string> RequiredActions { get; init; }
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
        IDocumentSession documentSession,
        ITokenProvider tokenProvider,
        IDateTimeProvider dateTimeProvider
    ) : ICommandHandler<Command, Response>
    {
        public async Task<ErrorOr<Response>> HandleAsync(Command command, CancellationToken cancellationToken)
        {
            var user = await documentSession.Query<User>()
                .Where(x => x.GuestId == command.GuestId)
                .FirstOrDefaultAsync(cancellationToken);

            if (user is null)
            {
                return UserErrors.NotFound;
            }

            var plainRefreshToken = tokenProvider.GenerateRefreshToken();

            var hashedRefreshToken = tokenProvider.HashRefreshToken(plainRefreshToken);

            var session = UserSession.Create(
                new StartSessionCommand(
                    user.Id,
                    hashedRefreshToken,
                    TimeSpan.FromDays(7),
                    dateTimeProvider.UtcNow
                )
            );

            documentSession.Store(session);

            await documentSession.SaveChangesAsync(cancellationToken);

            var accessToken = tokenProvider.GenerateAccessToken(user, session.Id);

            return new Response
            {
                AccessToken = accessToken,
                RefreshToken = plainRefreshToken,
                RequiredActions = user.RequiredActions.ToList(),
            };
        }
    }
}