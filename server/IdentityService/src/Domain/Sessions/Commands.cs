namespace Domain.Sessions;

public sealed record RotateSessionCommand(
    string NewRefreshTokenHash,
    TimeSpan ExtensionDuration,
    DateTime RotatedAt
);

public sealed record StartSessionCommand(
    string UserId,
    string RefreshTokenHash,
    TimeSpan Lifetime,
    DateTime StartedAt
);