using Application.Abstractions;
using Application.Abstractions.Messaging;
using Application.Abstractions.Providers;
using Application.Authentication;
using Confluent.Kafka;
using Domain;
using Domain.Learning;
using Domain.Quizzes;
using Domain.Users;
using Infrastructure.Authentication;
using Infrastructure.Clients;
using Infrastructure.Generators;
using Infrastructure.Messaging;
using Infrastructure.Options;
using Infrastructure.Projections;
using Infrastructure.QueueMessaging;
using Infrastructure.Subscriptions;
using Infrastructure.Subscriptions.Kafka;
using Infrastructure.Subscriptions.Kafka.Mapping;
using Infrastructure.Time;
using JasperFx.Core;
using JasperFx.Events;
using JasperFx.Events.Daemon;
using JasperFx.Events.Projections;
using Marten;
using Marten.Events.Projections;
using Marten.Schema;
using Marten.Storage;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.Google;
using Microsoft.SemanticKernel.Connectors.Qdrant;
using Neo4j.Driver;
using Npgsql;
using Qdrant.Client;
using Scrutor;
using Weasel.Core;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services
            .AddServices()
            .AddPostgres()
            .AddAuthenticationInternal()
            .AddKafka()
            .AddAuthorization()
            .AddNeo4J()
            .AddQdrant()
            .AddGoogleAi();

        return services;
    }

    private static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();

        services.Scan(scan => scan
            .FromAssembliesOf(typeof(KafkaSubscription))
            .AddClasses(classes => classes.AssignableTo<IIntegrationEventMapper>(), publicOnly: false)
            .UsingRegistrationStrategy(RegistrationStrategy.Append)
            .AsImplementedInterfaces()
            .WithSingletonLifetime());

        services.AddHostedService<UserOnboardingCompletedConsumer>();

        return services;
    }

    private static IServiceCollection AddKafka(this IServiceCollection services)
    {
        services.AddOptions<KafkaOptions>()
            .BindConfiguration(KafkaOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddSingleton<IProducer<string, string>>(sp =>
        {
            KafkaOptions options = sp.GetRequiredService<IOptions<KafkaOptions>>().Value;

            var config = new ProducerConfig
            {
                BootstrapServers = options.BootstrapServers,
                SecurityProtocol = SecurityProtocol.SaslPlaintext,
                SaslMechanism = SaslMechanism.ScramSha512,
                SaslUsername = options.Username,
                SaslPassword = options.Password,
                Acks = Acks.All,
                EnableIdempotence = true,
                MessageSendMaxRetries = 3
            };

            return new ProducerBuilder<string, string>(config).Build();
        });

        services.AddSingleton<IMessagePublisher, MessagePublisher>();

        return services;
    }

    private static IServiceCollection AddPostgres(this IServiceCollection services)
    {
        services
            .AddOptions<PostgresOptions>()
            .BindConfiguration(PostgresOptions.SectionName);

        services.AddMarten(serviceProvider =>
            {
                PostgresOptions postgresOptions = serviceProvider.GetRequiredService<IOptions<PostgresOptions>>().Value;

                var connectionStringBuilder = new NpgsqlConnectionStringBuilder
                {
                    Host = postgresOptions.Host,
                    Port = postgresOptions.Port,
                    Database = postgresOptions.Database,
                    Username = postgresOptions.Username,
                    Password = postgresOptions.Password,
                };

                var options = new StoreOptions
                {
                    Projections =
                    {
                        UseIdentityMapForAggregates = true,
                    },
                    Events =
                    {
                        StreamIdentity = StreamIdentity.AsString,
                        AppendMode = EventAppendMode.Quick,
                        TenancyStyle = TenancyStyle.Single,
                        UseMandatoryStreamTypeDeclaration = true
                    }
                };

                options.Policies.ForAllDocuments(mapping => { mapping.Alias = mapping.DocumentType.Name.ToCamelCase(); });

                options.Connection(connectionStringBuilder.ConnectionString);

                options.UseSystemTextJsonForSerialization(enumStorage: EnumStorage.AsString, Casing.SnakeCase);

                options.Projections.Snapshot<Quiz>(SnapshotLifecycle.Inline);
                options.Projections.Snapshot<TopicMastery>(SnapshotLifecycle.Inline);
                options.Projections.Add<QuizSummaryViewProjection>(ProjectionLifecycle.Async);
                options.Projections.Add<UserDashboardProjection>(ProjectionLifecycle.Async);

                options.Schema.For<User>().Identity(x => x.Id);

                return options;
            })
            .UseLightweightSessions()
            .AddSubscriptionWithServices<GraphSyncSubscription>(ServiceLifetime.Singleton)
            .AddSubscriptionWithServices<MasteryCalculationSubscription>(ServiceLifetime.Singleton)
            .AddSubscriptionWithServices<KafkaSubscription>(ServiceLifetime.Singleton)
            .AddAsyncDaemon(DaemonMode.HotCold);

        return services;
    }

    private static IServiceCollection AddAuthenticationInternal(
        this IServiceCollection services
    )
    {
        services.AddHttpContextAccessor();

        services.AddScoped<IUserContext, UserContext>();

        return services;
    }

    private static IServiceCollection AddNeo4J(this IServiceCollection services)
    {
        services.AddOptions<Neo4JOptions>()
            .BindConfiguration(Neo4JOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddSingleton<IDriver>(sp =>
        {
            Neo4JOptions options = sp.GetRequiredService<IOptions<Neo4JOptions>>().Value;
            return GraphDatabase.Driver(options.Uri, AuthTokens.Basic(options.Username, options.Password));
        });

        services.AddScoped<IKnowledgeGraphClient, Neo4JKnowledgeGraphClient>();

        return services;
    }

    private static IServiceCollection AddQdrant(this IServiceCollection services)
    {
        services
            .AddOptions<QdrantOptions>()
            .BindConfiguration(QdrantOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddSingleton<VectorStore>(sp =>
        {
            QdrantOptions options = sp.GetRequiredService<IOptions<QdrantOptions>>().Value;

            QdrantClient qdrantClient = new QdrantClient(options.Host, options.Port, options.Https, options.ApiKey);

            return new QdrantVectorStore(qdrantClient, ownsClient: true);
        });

        services.AddScoped<ISemanticContextClient, QdrantSemanticContextClient>();

        return services;
    }

    private static IServiceCollection AddGoogleAi(this IServiceCollection services)
    {
        services.AddOptions<GoogleAiOptions>()
            .BindConfiguration(GoogleAiOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddSingleton<IChatCompletionService>(sp =>
        {
            GoogleAiOptions options = sp.GetRequiredService<IOptions<GoogleAiOptions>>().Value;

            return new GoogleAIGeminiChatCompletionService(
                modelId: options.ChatModelId,
                apiKey: options.ApiKey
            );
        });

        services.AddSingleton<IEmbeddingGenerator<string, Embedding<float>>>(sp =>
        {
            GoogleAiOptions options = sp.GetRequiredService<IOptions<GoogleAiOptions>>().Value;

            return new GoogleAIEmbeddingGenerator(
                apiKey: options.ApiKey,
                modelId: options.EmbeddingModelId,
                dimensions: 768
            );
        });

        services.AddScoped<IStructuredContentGenerator, GeminiContentGenerator>();

        return services;
    }
}