using System.Text.Json;
using Application.Abstractions.Messaging;
using Application.Quizzes;
using Application.Quizzes.Commands;
using Confluent.Kafka;
using Infrastructure.Options;
using Messaging.Contracts.IntegrationEvents.Activities;
using Messaging.Contracts.Topology;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Serilog.Context;
using Serilog.Core.Enrichers;

namespace ActivityService.QuizActivity.Worker;

public partial class QuizActivityGenerationConsumer(
    ILogger<QuizActivityGenerationConsumer> logger,
    IServiceScopeFactory scopeFactory,
    IOptions<KafkaOptions> kafkaOptions
) : BackgroundService
{
    private readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    private readonly KafkaOptions _kafkaOptions = kafkaOptions.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var config = new ConsumerConfig
        {
            BootstrapServers = _kafkaOptions.BootstrapServers,
            GroupId = Topology.ConsumerGroups.ActivityServiceQuizActivityGenerator,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false,
            SecurityProtocol = SecurityProtocol.SaslPlaintext,
            SaslMechanism = SaslMechanism.ScramSha512,
            SaslUsername = _kafkaOptions.Username,
            SaslPassword = _kafkaOptions.Password
        };

        using IConsumer<Ignore, string> consumer = new ConsumerBuilder<Ignore, string>(config).Build();

        consumer.Subscribe(Topology.Topics.ActivityLearningActivityRequested);

        LogConsumerStarted(logger);

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                ConsumeResult<Ignore, string> consumeResult = consumer.Consume(stoppingToken);

                if (consumeResult == null)
                {
                    continue;
                }

                using IDisposable eventScope = LogContext.Push(
                    new PropertyEnricher("Topic", consumeResult.Topic),
                    new PropertyEnricher("Partition", consumeResult.Partition.Value),
                    new PropertyEnricher("Offset", consumeResult.Offset.Value)
                );

                LogProcessingEvent(logger);

                LogReceivedMessage(logger, consumeResult.Message.Value);

                try
                {
                    QuizActivityRequestedIntegrationEvent? integrationEvent = JsonSerializer.Deserialize<QuizActivityRequestedIntegrationEvent>(
                        consumeResult.Message.Value,
                        _jsonSerializerOptions
                    );

                    if (integrationEvent != null)
                    {
                        using IDisposable domainScope = LogContext.Push(
                            new PropertyEnricher("EventId", integrationEvent.Id),
                            new PropertyEnricher("ActivityId", integrationEvent.ActivityId)
                        );

                        using IServiceScope scope = scopeFactory.CreateScope();

                        ICommandHandler<GenerateQuiz.Command> commandHandler = scope.ServiceProvider.GetRequiredService<ICommandHandler<GenerateQuiz.Command>>();

                        var command = new GenerateQuiz.Command(integrationEvent.ActivityId);

                        await commandHandler.HandleAsync(command, stoppingToken);
                    }

                    consumer.Commit(consumeResult);

                    LogSuccessfullyProcessed(logger);
                }
                catch (JsonException jsonEx)
                {
                    LogSerializationFailed(logger, jsonEx);
                    
                    consumer.Commit(consumeResult);
                }
                catch (Exception ex)
                {
                    LogUnhandledError(logger, ex);
                }
            }
        }
        catch (OperationCanceledException ex)
        {
            LogGracefulShutdown(logger, ex);
        }
        finally
        {
            consumer.Close();
        }
    }

    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "Kafka Consumer started listening for learning activity events.")]
    private static partial void LogConsumerStarted(ILogger logger);

    [LoggerMessage(EventId = 2, Level = LogLevel.Debug, Message = "Processing Kafka event")]
    private static partial void LogProcessingEvent(ILogger logger);

    [LoggerMessage(EventId = 3, Level = LogLevel.Information, Message = "Received Kafka message: {MessageValue}")]
    private static partial void LogReceivedMessage(ILogger logger, string messageValue);

    [LoggerMessage(EventId = 4, Level = LogLevel.Debug, Message = "Successfully processed and committed event")]
    private static partial void LogSuccessfullyProcessed(ILogger logger);

    [LoggerMessage(EventId = 5, Level = LogLevel.Error, Message = "Serialization failed, invalid JSON format. Sending to dead letter / skipping")]
    private static partial void LogSerializationFailed(ILogger logger, Exception ex);

    [LoggerMessage(EventId = 6, Level = LogLevel.Critical, Message = "Unhandled error during event processing. Offset will not be committed.")]
    private static partial void LogUnhandledError(ILogger logger, Exception ex);

    [LoggerMessage(EventId = 7, Level = LogLevel.Information, Message = "Gracefully stopping the Kafka Consumer.")]
    private static partial void LogGracefulShutdown(ILogger logger, Exception ex);
}
