using System.Text.Json;
using Application.Abstractions.Messaging;
using Confluent.Kafka;
using Infrastructure.Subscriptions.Kafka.Mapping;
using JasperFx.Events;
using JasperFx.Events.Daemon;
using JasperFx.Events.Projections;
using Marten;
using Marten.Subscriptions;
using Microsoft.Extensions.Logging;
using Serilog.Context;
using Serilog.Core.Enrichers;

namespace Infrastructure.Subscriptions.Kafka;

public partial class KafkaSubscription : SubscriptionBase
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

        foreach (Type type in _eventMappers.Keys)
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
        long lastProcessed = page.SequenceFloor;

        using IDisposable pageScope = LogContext.PushProperty("SequenceFloor", page.SequenceFloor);
        
        using IDisposable ceilScope = LogContext.PushProperty("SequenceCeiling", page.SequenceCeiling);

        LogProcessingEventPage();

        foreach (IEvent @event in page.Events)
        {
            using IDisposable eventScope = LogContext.Push(
                new PropertyEnricher("EventId", @event.Id),
                new PropertyEnricher("EventType", @event.Data.GetType().Name),
                new PropertyEnricher("Sequence", @event.Sequence),
                new PropertyEnricher("StreamId", @event.StreamId)
            );

            if (!_eventMappers.TryGetValue(@event.Data.GetType(), out IIntegrationEventMapper? route))
            {
                LogNoRouteFound();
                
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

                LogPublishedToTopic(route.Topic);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (JsonException ex)
            {
                LogSerializationFailed(ex);
                
                await controller.RecordDeadLetterEventAsync(@event, ex);
            }
            catch (ProduceException<string, string> ex)
            {
                LogKafkaPublishFailed(ex);
                
                await controller.ReportCriticalFailureAsync(ex, lastProcessed);
                
                break;
            }
            catch (Exception ex)
            {
                LogUnhandledError(ex);
                
                await controller.ReportCriticalFailureAsync(ex, lastProcessed);
                
                break;
            }
        }

        LogPageComplete(lastProcessed);

        return NullChangeListener.Instance;
    }


    [LoggerMessage(EventId = 1, Level = LogLevel.Debug, Message = "Processing event page")]
    private partial void LogProcessingEventPage();

    [LoggerMessage(EventId = 2, Level = LogLevel.Debug, Message = "No route found, skipping event")]
    private partial void LogNoRouteFound();

    [LoggerMessage(EventId = 3, Level = LogLevel.Debug, Message = "Published to {Topic}")]
    private partial void LogPublishedToTopic(string topic);

    [LoggerMessage(EventId = 4, Level = LogLevel.Error, Message = "Serialization failed, sending to dead letter")]
    private partial void LogSerializationFailed(Exception ex);

    [LoggerMessage(EventId = 5, Level = LogLevel.Critical, Message = "Kafka publish failed, pausing subscription")]
    private partial void LogKafkaPublishFailed(Exception ex);

    [LoggerMessage(EventId = 6, Level = LogLevel.Critical, Message = "Unhandled error, pausing subscription")]
    private partial void LogUnhandledError(Exception ex);

    [LoggerMessage(EventId = 7, Level = LogLevel.Debug, Message = "Page complete, last processed {LastProcessed}")]
    private partial void LogPageComplete(long lastProcessed);
}
