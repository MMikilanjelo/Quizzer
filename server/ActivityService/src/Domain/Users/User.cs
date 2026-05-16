namespace Domain.Users;

public sealed record User
{
    public required string Id { get; init; }
    public required List<Goal> Goals { get; init; }
    public required List<Interest> Interests { get; init; }
    public required Proficiency Proficiency { get; init; }
    public required DateTime OnboardedAt { get; init; }
}