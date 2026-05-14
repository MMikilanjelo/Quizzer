using Domain.Learning;

namespace Application.Abstractions;

public record DiscoveryNode(string Id, string Name, string Domain, double Mastery);

public interface IKnowledgeGraphClient
{
    Task<string> GetGraphContextAsync(
        IEnumerable<string> nodeIds,
        CancellationToken cancellationToken
    );

    Task<List<DiscoveryNode>> GetDiscoveryNodesAsync(
        string topicId,
        string userId,
        int limit,
        CancellationToken cancellationToken
    );

    Task UpdateMasteryEdgeAsync(
        ConceptMasteryUpdated masteryEvent,
        CancellationToken cancellationToken
    );

    Task<List<string>> GetAvailableDomainsAsync(CancellationToken cancellationToken);
}