using JasperFx.Events;
using Messaging.Contracts.Abstractions;

namespace Infrastructure.Subscriptions.Kafka.Mapping;

internal abstract class IntegrationEventMapper<TFrom> : IIntegrationEventMapper
{
    public Type EventType => typeof(TFrom);
    public abstract string Topic { get; }

    public IIntegrationEvent Map(IEvent @event) =>
        Map((TFrom)@event.Data);

    protected abstract IIntegrationEvent Map(TFrom @event);
}