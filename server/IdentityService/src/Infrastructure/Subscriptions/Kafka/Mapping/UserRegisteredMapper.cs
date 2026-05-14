using Domain.Users;
using Messaging.Contracts.Abstractions;
using Messaging.Contracts.IntegrationEvents.Identity;
using Messaging.Contracts.Topology;

namespace Infrastructure.Subscriptions.Kafka.Mapping;

internal sealed class UserRegisteredMapper : IntegrationEventMapper<UserRegistered>
{
    public override string Topic => Topology.Topics.IdentityUserRegistered;

    protected override IIntegrationEvent Map(UserRegistered @event) =>
        new UserRegisteredIntegrationEvent(@event.UserId);
}