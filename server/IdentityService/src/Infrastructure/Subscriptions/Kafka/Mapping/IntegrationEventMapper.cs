using JasperFx.Events;
using Messaging.Contracts.Abstractions;

namespace Infrastructure.Subscriptions.Kafka.Mapping;

internal abstract class IntegrationEventMapper<TEvent> : IIntegrationEventMapper
{
    public Type EventType => typeof(TEvent);
    public abstract string Topic { get; }

    public IIntegrationEvent Map(IEvent @event) =>
        Map((TEvent)@event.Data);

    protected abstract IIntegrationEvent Map(TEvent @event);
}