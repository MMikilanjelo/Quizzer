using Messaging.Contracts.Abstractions;

namespace Messaging.Contracts.IntegrationEvents.Identity;

public sealed record UserOnboardingCompletedIntegrationEvent(
    string UserId,
    List<string> Goals,
    List<string> Interests,
    string Proficiency,
    DateTime CompletedAt
) : IntegrationEvent;