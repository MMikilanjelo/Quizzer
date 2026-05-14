using Application.Abstractions.Messaging;
using Application.Abstractions.Providers;
using Application.Authentication;
using Domain.Sessions;
using Domain.Users;
using ErrorOr;
using Marten;

namespace Application.Users.Commands;

public static class RefreshAccessToken
{
    public record Command : ICommand<Response>
    {
        public required string RefreshToken { get; init; }
    }

    public record Response
    {
        public required string AccessToken { get; init; }
        public required string RefreshToken { get; init; }
    }

    internal sealed class Handler(
        IDocumentSession documentSession,
        ITokenProvider tokenProvider,
        IDateTimeProvider dateTimeProvider
    ) : ICommandHandler<Command, Response>
    {
        public async Task<ErrorOr<Response>> HandleAsync(Command command, CancellationToken cancellationToken)
        {
            var incomingHash = tokenProvider.HashRefreshToken(command.RefreshToken);

            var session = await documentSession
                .Query<UserSession>()
                .FirstOrDefaultAsync(s => s.Token == incomingHash, cancellationToken);

            if (session is null)
            {
                return UserSessionErrors.RefreshTokenInvalid;
            }

            var newPlainToken = tokenProvider.GenerateRefreshToken();

            var newHashedToken = tokenProvider.HashRefreshToken(newPlainToken);

            var rotationResult = session.Rotate(
                new RotateSessionCommand(
                    newHashedToken,
                    TimeSpan.FromDays(7),
                    dateTimeProvider.UtcNow
                )
            );

            if (rotationResult.IsError)
            {
                documentSession.Delete(session);

                await documentSession.SaveChangesAsync(cancellationToken);

                return rotationResult.Errors;
            }

            var user = await documentSession
                .Query<User>()
                .FirstOrDefaultAsync(u => u.Id == session.UserId, cancellationToken);

            if (user is null)
            {
                return UserErrors.NotFound;
            }

            documentSession.Update(session);

            await documentSession.SaveChangesAsync(cancellationToken);

            var newAccessToken = tokenProvider.GenerateAccessToken(user, session.Id);

            return new Response
            {
                AccessToken = newAccessToken,
                RefreshToken = newPlainToken
            };
        }
    }
}