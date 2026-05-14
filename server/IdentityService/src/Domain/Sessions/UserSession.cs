using ErrorOr;

namespace Domain.Sessions;

public sealed record UserSession
{
    public required string Id { get; init; }
    public required string UserId { get; init; }
    public required string Token { get; init; }
    public required DateTime ExpiresAt { get; init; }
    public required bool IsRevoked { get; init; }
    public required DateTime CreatedAt { get; init; }

    private bool IsValid(DateTime now) =>
        !IsRevoked && ExpiresAt >= now;

    public static UserSession Create(StartSessionCommand command)
    {
        return new UserSession
        {
            Id = Ulid.NewUlid().ToString(),
            UserId = command.UserId,
            Token = command.RefreshTokenHash,
            CreatedAt = command.StartedAt,
            ExpiresAt = command.StartedAt.Add(command.Lifetime),
            IsRevoked = false
        };
    }

    public ErrorOr<UserSession> Rotate(RotateSessionCommand rotateSessionCommand)
    {
        if (!IsValid(rotateSessionCommand.RotatedAt))
        {
            return UserSessionErrors.SessionExpired;
        }

        return this with
        {
            Token = rotateSessionCommand.NewRefreshTokenHash,
            ExpiresAt = rotateSessionCommand.RotatedAt.Add(rotateSessionCommand.ExtensionDuration)
        };
    }

    public UserSession Revoke() =>
        this with { IsRevoked = true };
}