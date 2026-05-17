using Domain.Learning;

namespace Application.Abstractions;

public record TopicNode(string Id, string Name, string Domain, double Mastery);

public interface IKnowledgeGraphClient
{
    Task<List<string>> GetGlobalPriorityDomainsAsync(
        string userId,
        int limit,
        CancellationToken cancellationToken
    );

    Task<string> GetGraphContextAsync(
        IEnumerable<string> nodeIds,
        CancellationToken cancellationToken
    );

    Task<List<TopicNode>> GetTopicNodesAsync(
        string topicId,
        string userId,
        int limit,
        CancellationToken cancellationToken
    );

    Task UpdateMasteryEdgeAsync(
        TopicMasteryUpdated masteryEvent,
        CancellationToken cancellationToken
    );

    Task<List<string>> GetAvailableDomainsAsync(CancellationToken cancellationToken);
}