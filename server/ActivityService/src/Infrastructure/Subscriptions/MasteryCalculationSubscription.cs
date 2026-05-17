using Domain.Learning;
using Domain.Quizzes;
using JasperFx.Events;
using JasperFx.Events.Daemon;
using JasperFx.Events.Projections;
using Marten;
using Marten.Subscriptions;

namespace Infrastructure.Subscriptions;

public class MasteryCalculationSubscription : SubscriptionBase
{
    public MasteryCalculationSubscription()
    {
        Name = "MasteryCalculation";
        IncludeType<QuizQuestionAnswered>();
        Options.BatchSize = 100;
    }

    public override async Task<IChangeListener> ProcessEventsAsync(
        EventRange page,
        ISubscriptionController controller,
        IDocumentOperations ops,
        CancellationToken ct)
    {
        var events = page.Events
            .OfType<IEvent<QuizQuestionAnswered>>()
            .GroupBy(x => TopicMastery.FormatId(x.Data.UserId, x.Data.ConceptId));

        foreach (var group in events)
        {
            var streamId = group.Key;

            var mastery = await ops.Events.AggregateStreamAsync<TopicMastery>(streamId, token: ct);

            var toAppend = new List<object>();
            var first = group.First().Data;

            if (mastery is null)
            {
                var started = new TopicMasteryStarted(
                    first.UserId,
                    first.ConceptId,
                    BktParams.Initial,
                    first.AnsweredAt);

                toAppend.Add(started);
                mastery = TopicMastery.Create(started);
            }

            foreach (var e in group)
            {
                var answer = e.Data;
                var updatedEvent = mastery.RecordAttempt(
                    answer.IsCorrect,
                    answer.QuizId,
                    answer.QuestionId,
                    answer.AnsweredAt);

                toAppend.Add(updatedEvent);

                mastery = mastery.Apply(updatedEvent);
            }

            ops.Events.Append(streamId, toAppend.ToArray());
        }

        return NullChangeListener.Instance;
    }
}