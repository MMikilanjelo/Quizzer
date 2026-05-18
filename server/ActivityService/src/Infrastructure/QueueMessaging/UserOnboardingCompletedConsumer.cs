using System.Text.Json;
using Application.Abstractions.Messaging;
using Application.Quizzes.Commands;
using Application.Users;
using Application.Users.Commands;
using Confluent.Kafka;
using Infrastructure.Options;
using Messaging.Contracts.IntegrationEvents.Activities;
using Messaging.Contracts.IntegrationEvents.Identity;
using Messaging.Contracts.Topology;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Serilog.Context;
using Serilog.Core.Enrichers;

namespace Infrastructure.QueueMessaging;

public partial class UserOnboardingCompletedConsumer(
    ILogger<UserOnboardingCompletedConsumer> logger,
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
            GroupId = Topology.ConsumerGroups.ActivityServiceUserOnboarding,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false,
            SecurityProtocol = SecurityProtocol.SaslPlaintext,
            SaslMechanism = SaslMechanism.ScramSha512,
            SaslUsername = _kafkaOptions.Username,
            SaslPassword = _kafkaOptions.Password
        };

        using IConsumer<Ignore, string> consumer = new ConsumerBuilder<Ignore, string>(config).Build();

        consumer.Subscribe(Topology.Topics.IdentityUserOnboardingCompleted);

        LogConsumerSubscribed(logger, Topology.Topics.IdentityUserOnboardingCompleted);

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

                try
                {
                    var integrationEvent = JsonSerializer.Deserialize<UserOnboardingCompletedIntegrationEvent>(
                        consumeResult.Message.Value,
                        _jsonSerializerOptions
                    );

                    if (integrationEvent != null)
                    {
                        using IDisposable domainScope = LogContext.Push(
                            new PropertyEnricher("EventId", integrationEvent.Id),
                            new PropertyEnricher("UserId", integrationEvent.UserId)
                        );

                        using IServiceScope scope = scopeFactory.CreateScope();

                        var commandHandler = scope.ServiceProvider.GetRequiredService<ICommandHandler<CreateUser.Command>>();

                        var command = new CreateUser.Command
                        {
                            UserId = integrationEvent.UserId,
                            Goals = integrationEvent.Goals,
                            Interests = integrationEvent.Interests,
                            Proficiency = integrationEvent.Proficiency,
                            OnboardedAt = integrationEvent.CompletedAt
                        };

                        var result = await commandHandler.HandleAsync(command, stoppingToken);

                        if (result.IsError)
                        {
                            LogDomainInitializationFailed(logger, integrationEvent.UserId, result.Errors);
                        }
                        else
                        {
                            LogSuccessfullyPersistedUser(logger, integrationEvent.UserId);
                        }

                        consumer.Commit(consumeResult);
                    }
                    else
                    {
                        LogEmptyPayloadWarning(logger);
                        consumer.Commit(consumeResult);
                    }
                }
                catch (JsonException jsonEx)
                {
                    LogPoisonPillDeserializationFailed(logger, jsonEx);
                    consumer.Commit(consumeResult);
                }
                catch (Exception ex)
                {
                    LogUnhandledSystemError(logger, ex);
                }
            }
        }
        catch (OperationCanceledException)
        {
            LogStreamShutdownGracefully(logger);
        }
        finally
        {
            consumer.Close();
            LogConsumerSessionDisconnected(logger);
        }
    }

    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "Kafka consumer subscribed to onboarding topic: {Topic}")]
    private static partial void LogConsumerSubscribed(ILogger logger, string topic);

    [LoggerMessage(EventId = 2, Level = LogLevel.Error, Message = "Domain context initialization failed for UserId {UserId}. Errors: {Errors}")]
    private static partial void LogDomainInitializationFailed(ILogger logger, string userId, object errors);

    [LoggerMessage(EventId = 3, Level = LogLevel.Information, Message = "Successfully persisted user context and initialized dashboard view for UserId: {UserId}")]
    private static partial void LogSuccessfullyPersistedUser(ILogger logger, string userId);

    [LoggerMessage(EventId = 4, Level = LogLevel.Warning, Message = "Received empty or un-parsable onboarding message payload.")]
    private static partial void LogEmptyPayloadWarning(ILogger logger);

    [LoggerMessage(EventId = 5, Level = LogLevel.Error, Message = "Poison pill encountered! Failed to deserialize onboarding payload JSON.")]
    private static partial void LogPoisonPillDeserializationFailed(ILogger logger, Exception ex);

    [LoggerMessage(EventId = 6, Level = LogLevel.Error, Message = "Unhandled system error executing onboarding persistence. Retrying message stream execution.")]
    private static partial void LogUnhandledSystemError(ILogger logger, Exception ex);

    [LoggerMessage(EventId = 7, Level = LogLevel.Information, Message = "Onboarding consumer stream processing shutdown gracefully via cancellation token.")]
    private static partial void LogStreamShutdownGracefully(ILogger logger);

    [LoggerMessage(EventId = 8, Level = LogLevel.Information, Message = "Onboarding Kafka consumer session securely disconnected.")]
    private static partial void LogConsumerSessionDisconnected(ILogger logger);
}