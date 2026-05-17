using Application.Abstractions;
using Domain.Learning;
using JasperFx.Events.Daemon;
using JasperFx.Events.Projections;
using Marten;
using Marten.Subscriptions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Neo4j.Driver;

namespace Infrastructure.Subscriptions;

public class GraphSyncSubscription : SubscriptionBase
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<GraphSyncSubscription> _logger;

    public GraphSyncSubscription(
        IServiceScopeFactory scopeFactory,
        ILogger<GraphSyncSubscription> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;

        IncludeType<TopicMasteryUpdated>();

        Name = "Neo4jMasterySync";
        Options.BatchSize = 100;
    }

    public override async Task<IChangeListener> ProcessEventsAsync(
        EventRange page,
        ISubscriptionController controller,
        IDocumentOperations operations,
        CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var graphClient = scope.ServiceProvider.GetRequiredService<IKnowledgeGraphClient>();

        long lastProcessed = page.SequenceFloor;

        foreach (var @event in page.Events)
        {
            if (@event.Data is not TopicMasteryUpdated masteryEvent) continue;

            try
            {
                await graphClient.UpdateMasteryEdgeAsync(
                    masteryEvent,
                    cancellationToken
                );

                lastProcessed = @event.Sequence;
            }
            catch (Neo4jException ex)
            {
                _logger.LogError(ex, "Transient Neo4j error at sequence {Sequence}", @event.Sequence);

                await controller.ReportCriticalFailureAsync(ex, lastProcessed);
                
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unrecoverable error in graph sync for event {EventId}", @event.Id);
                
                await controller.RecordDeadLetterEventAsync(@event, ex);
                
                lastProcessed = @event.Sequence;
            }
        }

        return NullChangeListener.Instance;
    }
}