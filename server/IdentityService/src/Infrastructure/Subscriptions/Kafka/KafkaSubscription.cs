using System.Text.Json;
using Application.Abstractions.Messaging;
using Confluent.Kafka;
using Infrastructure.Subscriptions.Kafka.Mapping;
using JasperFx.Events.Daemon;
using JasperFx.Events.Projections;
using Marten;
using Marten.Subscriptions;
using Microsoft.Extensions.Logging;
using Serilog.Context;
using Serilog.Core.Enrichers;

namespace Infrastructure.Subscriptions.Kafka;

public class KafkaSubscription : SubscriptionBase
{
    private readonly IMessagePublisher _publisher;

    private readonly ILogger<KafkaSubscription> _logger;

    private readonly Dictionary<Type, IIntegrationEventMapper> _eventMappers;

    public KafkaSubscription(
        IMessagePublisher publisher,
        ILogger<KafkaSubscription> logger,
        IEnumerable<IIntegrationEventMapper> integrationEventMappers
    )
    {
        _publisher = publisher;

        _logger = logger;

        _eventMappers = integrationEventMappers.ToDictionary(x => x.EventType);

        foreach (var type in _eventMappers.Keys)
        {
            IncludeType(type);
        }

        Name = "KafkaOutbox";
        Options.BatchSize = 1000;
        Options.MaximumHopperSize = 10000;
        Options.SubscribeFromPresent();
    }

    public override async Task<IChangeListener> ProcessEventsAsync(
        EventRange page,
        ISubscriptionController controller,
        IDocumentOperations operations,
        CancellationToken cancellationToken
    )
    {
        var lastProcessed = page.SequenceFloor;

        using var pageScope = LogContext.PushProperty("SequenceFloor", page.SequenceFloor);

        using var ceilScope = LogContext.PushProperty("SequenceCeiling", page.SequenceCeiling);

        _logger.LogDebug("Processing event page");

        foreach (var @event in page.Events)
        {
            using var eventScope = LogContext.Push(
                new PropertyEnricher("EventId", @event.Id),
                new PropertyEnricher("EventType", @event.Data.GetType().Name),
                new PropertyEnricher("Sequence", @event.Sequence),
                new PropertyEnricher("StreamId", @event.StreamId)
            );

            if (!_eventMappers.TryGetValue(@event.Data.GetType(), out var route))
            {
                _logger.LogDebug("No route found, skipping event");
                lastProcessed = @event.Sequence;
                continue;
            }

            try
            {
                await _publisher.PublishAsync(
                    route.Map(@event),
                    route.Topic,
                    routingKey: @event.StreamId.ToString(),
                    cancellationToken
                );

                lastProcessed = @event.Sequence;

                _logger.LogDebug("Published to {Topic}", route.Topic);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Serialization failed, sending to dead letter");

                await controller.RecordDeadLetterEventAsync(@event, ex);
            }
            catch (ProduceException<string, string> ex)
            {
                _logger.LogCritical(ex, "Kafka publish failed, pausing subscription");

                await controller.ReportCriticalFailureAsync(ex, lastProcessed);

                break;
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Unhandled error, pausing subscription");

                await controller.ReportCriticalFailureAsync(ex, lastProcessed);

                break;
            }
        }

        _logger.LogDebug("Page complete, last processed {LastProcessed}", lastProcessed);

        return NullChangeListener.Instance;
    }
}