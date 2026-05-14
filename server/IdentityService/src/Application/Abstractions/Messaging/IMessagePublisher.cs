using Messaging.Contracts.Abstractions;

namespace Application.Abstractions.Messaging;

public interface IMessagePublisher
{
    Task PublishAsync<T>(T message, string topicName, string routingKey, CancellationToken cancellationToken = default) where T : IIntegrationEvent;
    Task PublishAsync<T>(T message, string topicName, CancellationToken cancellationToken = default) where T : IIntegrationEvent;
}