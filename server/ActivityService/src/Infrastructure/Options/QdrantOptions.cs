namespace Infrastructure.Options;

internal sealed record QdrantOptions
{
    public const string SectionName = "Qdrant";
    public required string Host { get; init; }
    public int Port { get; init; } = 6334;
    public bool Https { get; init; }
    public required string ApiKey { get; init; }
}
