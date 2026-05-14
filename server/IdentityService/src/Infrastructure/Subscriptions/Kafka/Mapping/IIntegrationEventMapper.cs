using JasperFx.Events;
using Messaging.Contracts.Abstractions;

namespace Infrastructure.Subscriptions.Kafka.Mapping;

public interface IIntegrationEventMapper
{
    Type EventType { get; }
    string Topic { get; }
    IIntegrationEvent Map(IEvent @event);
}