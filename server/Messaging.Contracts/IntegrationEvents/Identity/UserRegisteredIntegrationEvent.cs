using Messaging.Contracts.Abstractions;

namespace Messaging.Contracts.IntegrationEvents.Identity;

public record UserRegisteredIntegrationEvent(string UserId) : IntegrationEvent;
