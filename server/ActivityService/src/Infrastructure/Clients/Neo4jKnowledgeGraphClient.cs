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

    public async Task<string> GetAdaptiveQuizDomainAsync(string userId, CancellationToken cancellationToken)
    {
        var response = await driver.ExecutableQuery(@"
            MATCH (topic:Topic)
            WHERE topic.domain IS NOT NULL AND topic.domain <> ''
            
            OPTIONAL MATCH (u:User {id: $userId})-[k:KNOWS]->(topic)
            
            WITH topic,
                 COALESCE(k.p_learned, 0.0) AS mastery,
                 duration.inDays(COALESCE(k.last_updated, datetime() - duration('P30D')), datetime()).days AS days_since_seen
                 
            WITH topic,
                 mastery,
                 CASE 
                    WHEN mastery >= 0.95 THEN 0.0 
                    ELSE (1.0 - mastery) + (days_since_seen * 0.015) 
                 END AS topic_priority
                 
            RETURN topic.domain AS Domain,
                   AVG(topic_priority) AS DomainPriorityScore,
                   COUNT(CASE WHEN k IS NULL THEN 1 END) AS UnseenTopicsCount,
                   COUNT(CASE WHEN mastery < 0.95 THEN 1 END) AS UnmasteredCount,
                   COUNT(topic) AS TotalStructuralSize
                   
            ORDER BY DomainPriorityScore DESC, 
                     UnseenTopicsCount DESC, 
                     UnmasteredCount DESC, 
                     TotalStructuralSize DESC
            LIMIT 1")
            .WithParameters(new { userId })
            .WithConfig(new QueryConfig(database: DatabaseName))
            .WithMap(r => r["Domain"].As<string>())
            .ExecuteAsync(cancellationToken);

        return response.Result.First();
    }

    public async Task<List<TopicNode>> GetTopicNodesAsync(
        string domainId,
        string userId,
        int limit,
        CancellationToken cancellationToken
    )
    {
        var response = await driver.ExecutableQuery(@"
            MATCH (topic:Topic)
            WHERE toLower(topic.domain) = $domainId 
              AND toLower(topic.topic_id) <> $domainId

            OPTIONAL MATCH (u:User {id: $userId})-[k:KNOWS]->(topic)
            
            WITH topic, 
                 k IS NULL AS is_unseen,
                 COALESCE(k.p_learned, 0.0) AS mastery,
                 COALESCE(k.last_updated, datetime() - duration('P30D')) AS last_seen
            
            WITH topic, is_unseen, mastery, 
                 duration.inDays(last_seen, datetime()).days AS days_since_seen

            WITH topic, is_unseen, mastery, days_since_seen,
                 (1.0 - mastery) + (days_since_seen * 0.015) AS priority_score
            
            WHERE mastery < 0.95 AND (is_unseen OR days_since_seen > 0 OR mastery < 0.5)
            
            RETURN topic.topic_id AS Id, 
                   topic.name AS Name, 
                   topic.domain AS Domain,
                   mastery
            ORDER BY priority_score DESC
            LIMIT $limit")
            .WithParameters(new
            {
                domainId = domainId.ToLowerInvariant(),
                userId,
                limit = (long)limit
            })
            .WithConfig(new QueryConfig(database: DatabaseName))
            .WithMap(r => new TopicNode(
                r["Id"].As<string>(),
                r["Name"].As<string>(),
                r["Domain"].As<string>(),
                r["mastery"].As<double>()
            ))
            .ExecuteAsync(cancellationToken);

        return response.Result.ToList();
    }

    public async Task<string> GetGraphContextAsync(IEnumerable<string> topicIds, CancellationToken cancellationToken)
    {
        var idsList = topicIds.ToList();

        if (idsList.Count == 0)
        {
            return string.Empty;
        }

        var response = await driver.ExecutableQuery(@"
                MATCH (topic:Topic) WHERE topic.topic_id IN $ids
                MATCH path = (topic)-[:SUPER_TOPIC_OF|CONTRIBUTES_TO|EQUIVALENT*1..2]-(neighbor:Topic)
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
            return "No topological relationships found for these topics.";
        }

        return FormatTopologyResult(response.Result);
    }

    public async Task UpdateMasteryEdgeAsync(
        TopicMasteryUpdated masteryEvent,
        CancellationToken cancellationToken
    )
    {
        var upsertQuery = @"
        MERGE (u:User {id: $userId})
        MERGE (topic:Topic {topic_id: $topicId})
        MERGE (u)-[k:KNOWS]->(topic)
        
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
                topicId = masteryEvent.TopicId,
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
                MATCH (topic:Topic)
                WHERE topic.domain IS NOT NULL AND topic.domain <> '' 
                RETURN DISTINCT topic.domain AS Domain
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