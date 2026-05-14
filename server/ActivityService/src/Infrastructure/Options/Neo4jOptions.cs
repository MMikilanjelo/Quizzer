namespace Infrastructure.Options;

internal sealed record Neo4JOptions
{
    public const string SectionName = "Neo4j";
    public required string Uri { get; init; }
    public required string Username { get; init; }
    public required string Password { get; init; }
}