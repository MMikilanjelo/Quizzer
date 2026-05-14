using Microsoft.Extensions.VectorData;

namespace Infrastructure.Clients;
internal class ArticleModel
{
    [VectorStoreKey] public Guid Id { get; set; }

    [VectorStoreData(IsIndexed = true, StorageName = "tags")]
    public string Tag { get; set; } = string.Empty;

    [VectorStoreData(IsIndexed = true, StorageName = "parent_title")]
    public string ParentTitle { get; init; } = string.Empty;

    [VectorStoreData(StorageName = "source_url")]
    public string Url { get; init; } = string.Empty;

    [VectorStoreData(IsFullTextIndexed = true, StorageName = "chunk_content")]
    public string Content { get; init; } = string.Empty;

    [VectorStoreData(IsIndexed = true, StorageName = "node_id")]
    public string NodeId { get; init; } = string.Empty;

    [VectorStoreVector(
        768,
        DistanceFunction = DistanceFunction.CosineSimilarity,
        IndexKind = IndexKind.Hnsw,
        StorageName = "content_embedding")
    ]
    public ReadOnlyMemory<float> Vector { get; set; }
}