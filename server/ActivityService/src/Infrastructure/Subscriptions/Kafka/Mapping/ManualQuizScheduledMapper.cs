using Domain.Quizzes;
using Messaging.Contracts.Abstractions;
using Messaging.Contracts.IntegrationEvents.Activities;
using Messaging.Contracts.Topology;

namespace Infrastructure.Subscriptions.Kafka.Mapping;

internal sealed class ManualQuizScheduledMapper : IntegrationEventMapper<ManualQuizScheduled>
{
    public override string Topic => Topology.Topics.ActivityLearningActivityRequested;

    protected override IIntegrationEvent Map(ManualQuizScheduled @event)
    {
        return new QuizActivityRequestedIntegrationEvent(
            @event.QuizId,
            @event.UserId
        );
    }
}