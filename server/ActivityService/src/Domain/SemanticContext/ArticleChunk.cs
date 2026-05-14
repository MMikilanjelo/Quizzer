namespace Domain.SemanticContext;

public sealed record ArticleChunk
{
    public required string Title { get; init; }
    public required string Body { get; init; }
    public required string Url { get; init; }
    public required string NodeId { get; init; }
}
