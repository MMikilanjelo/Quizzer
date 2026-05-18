using Application.Abstractions.Messaging;
using Application.Quizzes.Commands;
using Infrastructure.Options;
using Messaging.Contracts.IntegrationEvents.Activities;
using Messaging.Contracts.Topology;
using Confluent.Kafka;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Serilog.Context;
using Serilog.Core.Enrichers;
using ErrorOr;

namespace ActivityService.QuizActivity.Worker;

public partial class QuizActivityGenerationRetryConsumer(
    ILogger<QuizActivityGenerationRetryConsumer> logger,
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
            GroupId = Topology.ConsumerGroups.ActivityServiceQuizActivityGeneratorRetry,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false,
            SecurityProtocol = SecurityProtocol.SaslPlaintext,
            SaslMechanism = SaslMechanism.ScramSha512,
            SaslUsername = _kafkaOptions.Username,
            SaslPassword = _kafkaOptions.Password
        };

        using IConsumer<Ignore, string> consumer = new ConsumerBuilder<Ignore, string>(config).Build();

        consumer.Subscribe(Topology.Topics.ActivityLearningActivityRequestedRetry);

        LogRetryConsumerStarted(logger);

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

                LogProcessingRetryEvent(logger);

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

                        LogWaitingForCooldown(logger, integrationEvent.ActivityId);

                        await Task.Delay(TimeSpan.FromMinutes(2), stoppingToken);

                        using var scope = scopeFactory.CreateScope();

                        var generateCommandHandler = scope.ServiceProvider.GetRequiredService<ICommandHandler<GenerateQuiz.Command>>();

                        var generateCommand = new GenerateQuiz.Command(integrationEvent.ActivityId);

                        var result = await generateCommandHandler.HandleAsync(generateCommand, stoppingToken);

                        if (result.IsError)
                        {
                            LogFinalGenerationFailure(logger, integrationEvent.ActivityId, result.Errors);

                            ICommandHandler<FailQuiz.Command> failCommandHandler = scope.ServiceProvider.GetRequiredService<ICommandHandler<FailQuiz.Command>>();

                            var failCommand = new FailQuiz.Command
                            {
                                QuizId = integrationEvent.ActivityId
                            };

                            await failCommandHandler.HandleAsync(failCommand, stoppingToken);
                        }
                    }

                    consumer.Commit(consumeResult);

                    LogSuccessfullyProcessedRetry(logger);
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

    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "Kafka Retry Consumer started listening for delayed learning activity events.")]
    private static partial void LogRetryConsumerStarted(ILogger logger);

    [LoggerMessage(EventId = 2, Level = LogLevel.Debug, Message = "Processing Kafka retry event")]
    private static partial void LogProcessingRetryEvent(ILogger logger);

    [LoggerMessage(EventId = 3, Level = LogLevel.Information, Message = "Delaying retry for Activity {ActivityId} to allow API cooldown.")]
    private static partial void LogWaitingForCooldown(ILogger logger, string activityId);

    [LoggerMessage(EventId = 4, Level = LogLevel.Error, Message = "Final retry failed for Activity {ActivityId}. Marking quiz as Failed in database. Errors: {Errors}")]
    private static partial void LogFinalGenerationFailure(ILogger logger, string activityId, object errors);

    [LoggerMessage(EventId = 5, Level = LogLevel.Information, Message = "Successfully processed and committed retry event")]
    private static partial void LogSuccessfullyProcessedRetry(ILogger logger);

    [LoggerMessage(EventId = 6, Level = LogLevel.Error, Message = "Serialization failed, invalid JSON format. Sending to dead letter / skipping")]
    private static partial void LogSerializationFailed(ILogger logger, Exception ex);

    [LoggerMessage(EventId = 7, Level = LogLevel.Critical, Message = "Unhandled error during retry event processing. Offset will not be committed.")]
    private static partial void LogUnhandledError(ILogger logger, Exception ex);

    [LoggerMessage(EventId = 8, Level = LogLevel.Information, Message = "Gracefully stopping the Kafka Retry Consumer.")]
    private static partial void LogGracefulShutdown(ILogger logger, Exception ex);
}