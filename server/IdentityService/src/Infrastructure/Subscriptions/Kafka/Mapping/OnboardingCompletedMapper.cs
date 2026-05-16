using Domain.Users;
using Messaging.Contracts.Abstractions;
using Messaging.Contracts.IntegrationEvents.Identity;
using Messaging.Contracts.Topology;

namespace Infrastructure.Subscriptions.Kafka.Mapping;

internal sealed class OnboardingCompletedMapper : IntegrationEventMapper<OnboardingCompleted>
{
    public override string Topic => Topology.Topics.IdentityUserOnboardingCompleted;

    protected override IIntegrationEvent Map(OnboardingCompleted @event) =>
        new UserOnboardingCompletedIntegrationEvent(
            @event.UserId,
            @event.Goals,
            @event.Interests,
            @event.ProficiencyLevel,
            @event.CompletedAt
        );
}