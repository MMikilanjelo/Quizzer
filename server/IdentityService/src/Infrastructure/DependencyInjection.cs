using Application.Abstractions;
using Application.Abstractions.Messaging;
using Application.Abstractions.Providers;
using Application.Authentication;
using Confluent.Kafka;
using Domain.Sessions;
using Domain.Users;
using Infrastructure.Authentication;
using Infrastructure.Messaging;
using Infrastructure.Options;
using Infrastructure.Projections;
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
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Npgsql;
using Scrutor;
using Weasel.Core;
using StoreOptions = Marten.StoreOptions;

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
            .AddAuthenticationInternal(configuration)
            .AddKafka()
            .AddAuthorization();

        return services;
    }

    private static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();

        services.Scan(scan => scan
            .FromAssembliesOf(typeof(KafkaSubscription))
            .AddClasses(classes => classes.AssignableTo<IIntegrationEventMapper>(), publicOnly: false)
            .UsingRegistrationStrategy(RegistrationStrategy.Skip)
            .AsImplementedInterfaces()
            .WithSingletonLifetime());

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
            var options = sp.GetRequiredService<IOptions<KafkaOptions>>().Value;

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

        services
            .AddMarten(serviceProvider =>
            {
                var postgresOptions = serviceProvider.GetRequiredService<IOptions<PostgresOptions>>().Value;

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
                        UseMandatoryStreamTypeDeclaration = true
                    }
                };
                options.Policies.ForAllDocuments(mapping =>
                {
                    mapping.Alias = mapping.DocumentType.Name.ToCamelCase();
                });
                options.Connection(connectionStringBuilder.ConnectionString);
                options.UseSystemTextJsonForSerialization(enumStorage: EnumStorage.AsString, Casing.SnakeCase);
                
                options.Projections.Snapshot<User>(SnapshotLifecycle.Inline);
                options.Projections.Add<UserProfileViewProjection>(ProjectionLifecycle.Async);

                options.Schema
                    .For<User>()
                    .Identity(x => x.Id)
                    .UniqueIndex(UniqueIndexType.Computed, x => x.GuestId);

                options.Schema
                    .For<UserSession>()
                    .Identity(x => x.Id)
                    .Index(x => x.Token)
                    .Index(x => x.UserId);

                return options;
            })
            .UseLightweightSessions()
            .AddSubscriptionWithServices<KafkaSubscription>(ServiceLifetime.Singleton)
            .AddAsyncDaemon(DaemonMode.HotCold);

        return services;
    }

    private static IServiceCollection AddAuthenticationInternal(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services
            .AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .Validate(o => !string.IsNullOrWhiteSpace(o.Secret), "Secret is required")
            .Validate(o => o.Secret.Length >= 32, "Secret must be at least 32 chars")
            .Validate(o => o.ExpirationInMinutes > 0, "Expiration must be > 0")
            .ValidateOnStart();

        services.AddHttpContextAccessor();

        services.AddScoped<IUserContext, UserContext>();

        services.AddSingleton<IPasswordHasher, PasswordHasher>();

        services.AddSingleton<ITokenProvider, TokenProvider>();

        return services;
    }
}