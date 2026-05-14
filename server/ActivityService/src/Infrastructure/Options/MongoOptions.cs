namespace Infrastructure.Options;

internal sealed record MongoDbOptions
{
    public const string SectionName = "Mongo";
    public required string Host { get; init; }
    public int Port { get; init; } = 27017;
    public required string Database { get; init; }
    public required string Username { get; init; }
    public required string Password { get; init; }
}