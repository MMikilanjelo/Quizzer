using Application.Abstractions;
using Domain.SemanticContext;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;

namespace Infrastructure.Clients;

public class QdrantSemanticContextClient(
    VectorStore vectorStore,
    IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator
) : ISemanticContextClient
{
    private readonly VectorStoreCollection<Guid, ArticleModel> _collection = vectorStore.GetCollection<Guid, ArticleModel>("articles");

    public async Task<List<ArticleChunk>> SearchTopicAsync(string topic, int limit, CancellationToken cancellationToken)
    {
        await _collection.EnsureCollectionExistsAsync(cancellationToken);

        Embedding<float> queryEmbeddings = await embeddingGenerator.GenerateAsync(topic, cancellationToken: cancellationToken);

        IAsyncEnumerable<VectorSearchResult<ArticleModel>> searchResults = _collection.SearchAsync(
            queryEmbeddings.Vector,
            top: limit,
            new VectorSearchOptions<ArticleModel>
            {
                IncludeVectors = false,
            },
            cancellationToken
        );

        var chunks = await searchResults
            .Select(result => result.Record) 
            .Select(record => new ArticleChunk
            {
                Title = record.ParentTitle,
                Body = record.Content,
                Url = record.Url,
                NodeId = record.NodeId
            })
            .ToListAsync(cancellationToken);

        return chunks;
    }
}
