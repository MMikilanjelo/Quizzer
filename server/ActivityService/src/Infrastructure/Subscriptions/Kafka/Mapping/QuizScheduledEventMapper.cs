using Domain.Quizzes;
using Messaging.Contracts.Abstractions;
using Messaging.Contracts.IntegrationEvents.Activities;
using Messaging.Contracts.Topology;

namespace Infrastructure.Subscriptions.Kafka.Mapping;

internal sealed class QuizScheduledEventMapper : IntegrationEventMapper<QuizScheduled>
{
    public override string Topic => Topology.Topics.ActivityLearningActivityRequested;

    protected override IIntegrationEvent Map(QuizScheduled @event)
    {
        return new QuizActivityRequestedIntegrationEvent(
            @event.QuizId,
            @event.UserId
        );
    }
}