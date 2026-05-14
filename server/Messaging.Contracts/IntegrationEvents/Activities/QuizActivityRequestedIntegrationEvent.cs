using Messaging.Contracts.Abstractions;

namespace Messaging.Contracts.IntegrationEvents.Activities;

public sealed record QuizActivityRequestedIntegrationEvent(string ActivityId, string UserId) : IntegrationEvent;