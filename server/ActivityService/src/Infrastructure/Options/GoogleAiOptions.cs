namespace Infrastructure.Options;

internal sealed record GoogleAiOptions
{
    public const string SectionName = "GoogleAi";
    public required string ApiKey { get; init; }
    public string ChatModelId { get; init; } = "gemini-3-flash-preview";
    public string EmbeddingModelId { get; init; } = "gemini-embedding-001";
}