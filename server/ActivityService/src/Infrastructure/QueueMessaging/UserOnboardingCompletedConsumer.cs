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

public sealed class UserOnboardingCompletedConsumer(
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

        logger.LogInformation("Kafka consumer subscribed to onboarding topic: {Topic}", Topology.Topics.IdentityUserOnboardingCompleted);

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
                            logger.LogError("Domain context initialization failed for UserId {UserId}. Errors: {Errors}", integrationEvent.UserId, result.Errors);
                        }
                        else
                        {
                            logger.LogInformation("Successfully persisted user context and initialized dashboard view for UserId: {UserId}", integrationEvent.UserId);
                        }

                        consumer.Commit(consumeResult);
                    }
                    else
                    {
                        logger.LogWarning("Received empty or un-parsable onboarding message payload.");
                        consumer.Commit(consumeResult);
                    }
                }
                catch (JsonException jsonEx)
                {
                    logger.LogError(jsonEx, "Poison pill encountered! Failed to deserialize onboarding payload JSON.");
                    consumer.Commit(consumeResult);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Unhandled system error executing onboarding persistence. Retrying message stream execution.");
                }
            }
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("Onboarding consumer stream processing shutdown gracefully via cancellation token.");
        }
        finally
        {
            consumer.Close();
            logger.LogInformation("Onboarding Kafka consumer session securely disconnected.");
        }
    }
}