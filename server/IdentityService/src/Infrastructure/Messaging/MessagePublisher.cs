using System.Text.Json;
using Application.Abstractions.Messaging;
using Confluent.Kafka;
using Messaging.Contracts.Abstractions;

namespace Infrastructure.Messaging;

internal class MessagePublisher(IProducer<string, string> producer) : IMessagePublisher
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    public Task PublishAsync<T>(T message, string topicName, string routingKey, CancellationToken cancellationToken = default) where T : IIntegrationEvent
    {
        var json = JsonSerializer.Serialize(message, message.GetType(), JsonSerializerOptions);

        var kafkaMessage = new Message<string, string>
        {
            Key = routingKey,
            Value = json,
            Headers = new Headers
            {
                { "MessageId", message.Id.ToByteArray() },
                { "OccurredOnUtc", BitConverter.GetBytes(new DateTimeOffset(message.OccurredOnUtc).ToUnixTimeSeconds()) }
            }
        };

        return producer.ProduceAsync(topicName, kafkaMessage, cancellationToken);
    }

    public Task PublishAsync<T>(T message, string topicName, CancellationToken cancellationToken = default) where T : IIntegrationEvent =>
        PublishAsync(message, topicName, routingKey: string.Empty, cancellationToken);
}