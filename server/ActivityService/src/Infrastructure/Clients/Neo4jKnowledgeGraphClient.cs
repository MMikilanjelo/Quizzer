using System.Globalization;
using System.Text;
using Application.Abstractions;
using Domain.Learning;
using Microsoft.Extensions.Logging;
using Neo4j.Driver;

namespace Infrastructure.Clients;

public class Neo4JKnowledgeGraphClient(IDriver driver, ILogger<Neo4JKnowledgeGraphClient> logger) : IKnowledgeGraphClient
{
    private const string DatabaseName = "01f417e1";

    public async Task<List<string>> GetGlobalPriorityDomainsAsync(string userId, int limit, CancellationToken cancellationToken)
    {
        var personalizedResponse = await driver.ExecutableQuery(@"
            MATCH (u:User {id: $userId})-[k:KNOWS]->(sub:Topic)
            WHERE sub.domain IS NOT NULL AND sub.domain <> ''
            
            WITH sub.domain AS Domain,
                 COALESCE(k.p_learned, 0.0) AS mastery,
                 duration.inDays(COALESCE(k.last_updated, datetime() - duration('P30D')), datetime()).days AS days_since_seen
            
            WITH Domain,
                 (1.0 - mastery) + (days_since_seen * 0.015) AS node_priority
            
            RETURN Domain, 
                   AVG(node_priority) AS DomainPriorityScore
            ORDER BY DomainPriorityScore DESC
            LIMIT $limit")
            .WithParameters(new { userId, limit = (long)limit })
            .WithConfig(new QueryConfig(database: DatabaseName))
            .WithMap(r => r["Domain"].As<string>())
            .ExecuteAsync(cancellationToken);

        var domains = personalizedResponse.Result.ToList();

        if (domains.Any())
        {
            return domains;
        }

        var fallbackResponse = await driver.ExecutableQuery(@"
                MATCH (root:Topic)-[:SUPER_TOPIC_OF]->(sub:Topic)
                WHERE root.domain IS NOT NULL AND root.domain <> ''
                RETURN root.domain AS Domain, count(sub) AS Connections
                ORDER BY Connections DESC
                LIMIT $limit")
            .WithParameters(new { limit = (long)limit })
            .WithConfig(new QueryConfig(database: DatabaseName))
            .WithMap(r => r["Domain"].As<string>())
            .ExecuteAsync(cancellationToken);

        return fallbackResponse.Result.ToList();
    }

    public async Task<List<DiscoveryNode>> GetDiscoveryNodesAsync(
        string topicId,
        string userId,
        int limit,
        CancellationToken cancellationToken)
    {
        var response = await driver.ExecutableQuery(@"
            MATCH (sub:Topic)
            WHERE toLower(sub.domain) = $topicId 
               OR toLower(sub.topic_id) = $topicId
               OR EXISTS { (root:Topic)-[:SUPER_TOPIC_OF*1..3]->(sub) WHERE toLower(root.topic_id) = $topicId }

            OPTIONAL MATCH (u:User {id: $userId})-[k:KNOWS]->(sub)
            
            WITH sub, 
                 COALESCE(k.p_learned, 0.0) AS mastery,
                 COALESCE(k.last_updated, datetime() - duration('P30D')) AS last_seen
            
            WITH sub, mastery, 
                 duration.inDays(last_seen, datetime()).days AS days_since_seen

            WITH sub, mastery, days_since_seen,
                 (1.0 - mastery) + (days_since_seen * 0.015) AS priority_score
            
            WHERE mastery < 0.95 AND (days_since_seen > 0 OR mastery < 0.5)
            
            RETURN sub.topic_id AS Id, 
                   sub.name AS Name, 
                   sub.domain AS Domain,
                   mastery
            ORDER BY priority_score DESC
            LIMIT $limit")
            .WithParameters(new
            {
                topicId,
                userId,
                limit = (long)limit
            })
            .WithConfig(new QueryConfig(database: DatabaseName))
            .WithMap(r => new DiscoveryNode(
                r["Id"].As<string>(),
                r["Name"].As<string>(),
                r["Domain"].As<string>(),
                r["mastery"].As<double>()
            ))
            .ExecuteAsync(cancellationToken);

        return response.Result.ToList();
    }

    public async Task<string> GetGraphContextAsync(IEnumerable<string> nodeIds, CancellationToken cancellationToken)
    {
        var idsList = nodeIds.ToList();

        if (idsList.Count == 0)
        {
            return string.Empty;
        }

        var response = await driver.ExecutableQuery(@"
                MATCH (t:Topic) WHERE t.topic_id IN $ids
                MATCH path = (t)-[:SUPER_TOPIC_OF|CONTRIBUTES_TO|EQUIVALENT*1..2]-(neighbor:Topic)
                UNWIND relationships(path) AS rel
                WITH DISTINCT rel
                RETURN startNode(rel).topic_id AS TopicId, 
                       type(rel) AS RelType, 
                       endNode(rel).topic_id AS NeighborId
                LIMIT 50"
            )
            .WithParameters(new { ids = idsList })
            .WithConfig(new QueryConfig(database: DatabaseName))
            .ExecuteAsync(cancellationToken);

        if (!response.Result.Any())
        {
            return "No topological relationships found for these concepts.";
        }

        return FormatTopologyResult(response.Result);
    }

    public async Task UpdateMasteryEdgeAsync(
        ConceptMasteryUpdated masteryEvent,
        CancellationToken cancellationToken
    )
    {
        var upsertQuery = @"
        MERGE (u:User {id: $userId})
        MERGE (t:Topic {topic_id: $conceptId})
        MERGE (u)-[k:KNOWS]->(t)
        
        ON CREATE SET 
            k.p_learned = $pLearned,
            k.p_guess = $pGuess,
            k.p_slip = $pSlip,
            k.p_transit = $pTransit,
            k.last_updated = datetime($lastUpdated)
            
        ON MATCH SET 
            k.p_learned = CASE WHEN datetime($lastUpdated) > k.last_updated THEN $pLearned ELSE k.p_learned END,
            k.p_guess = CASE WHEN datetime($lastUpdated) > k.last_updated THEN $pGuess ELSE k.p_guess END,
            k.p_slip = CASE WHEN datetime($lastUpdated) > k.last_updated THEN $pSlip ELSE k.p_slip END,
            k.p_transit = CASE WHEN datetime($lastUpdated) > k.last_updated THEN $pTransit ELSE k.p_transit END,
            k.last_updated = CASE WHEN datetime($lastUpdated) > k.last_updated THEN datetime($lastUpdated) ELSE k.last_updated END";

        await driver.ExecutableQuery(upsertQuery)
            .WithParameters(new
            {
                userId = masteryEvent.UserId,
                conceptId = masteryEvent.ConceptId,
                pLearned = masteryEvent.NewMastery,
                pGuess = masteryEvent.PGuess,
                pSlip = masteryEvent.PSlip,
                pTransit = masteryEvent.PTransition,
                lastUpdated = masteryEvent.Timestamp.ToString("o")
            })
            .WithConfig(new QueryConfig(database: DatabaseName))
            .ExecuteAsync(cancellationToken);
    }

    public async Task<List<string>> GetAvailableDomainsAsync(CancellationToken cancellationToken)
    {
        var response = await driver
            .ExecutableQuery(@"
                MATCH (t:Topic)
                WHERE t.domain IS NOT NULL AND t.domain <> '' 
                RETURN DISTINCT t.domain AS Domain
                ORDER BY Domain ASC"
            )
            .WithConfig(new QueryConfig(database: DatabaseName))
            .WithMap(r => r["Domain"].As<string>())
            .ExecuteAsync(cancellationToken);

        return response.Result.ToList();
    }

    private static string FormatTopologyResult(IReadOnlyList<IRecord> records)
    {
        var sb = new StringBuilder();

        sb.AppendLine("### KNOWLEDGE GRAPH TOPOLOGY");

        sb.AppendLine("The following relationships define the curriculum hierarchy and technical tradeoffs:");

        var grouped = records.GroupBy(r => r["RelType"].As<string>());

        foreach (var group in grouped)
        {
            string description = GetRelationDescription(group.Key);

            sb.AppendLine(CultureInfo.InvariantCulture, $"\n**{group.Key} ({description}):**");

            foreach (var rec in group)
            {
                string topic = rec["TopicId"].As<string>();

                string neighbor = rec["NeighborId"].As<string>();

                sb.AppendLine(CultureInfo.InvariantCulture, $"- {topic} <-> {neighbor}");
            }
        }

        return sb.ToString();
    }

    private static string GetRelationDescription(string relType) => relType switch
    {
        "SUPER_TOPIC_OF" => "Hierarchy/Prerequisites",
        "CONTRIBUTES_TO" => "Technical Tradeoffs/Impacts",
        "EQUIVALENT" => "Synonyms/Related Terms",
        _ => "General Relation"
    };
}