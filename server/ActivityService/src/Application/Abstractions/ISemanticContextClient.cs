using Domain.SemanticContext;

namespace Application.Abstractions;

public interface ISemanticContextClient
{
    Task<List<ArticleChunk>> SearchTopicAsync(string topic, int limit, CancellationToken cancellationToken);
}
