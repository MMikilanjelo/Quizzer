namespace Messaging.Contracts.Topology;

public static class Topology
{
    /// <summary>
    /// Centralized registry for Kafka topic names across the ecosystem.
    /// </summary>
    /// <remarks>
    /// All topics must strictly follow the bounded-context naming convention:
    /// <para><c>[context].[aggregate].[event]</c></para>
    /// <list type="bullet">
    /// <item><description><b>context:</b> The originating microservice domain (e.g., <c>identity</c>, <c>activity</c>).</description></item>
    /// <item><description><b>aggregate:</b> The affected domain aggregate root (e.g., <c>user</c>, <c>learning-activity</c>).</description></item>
    /// <item><description><b>event:</b> The specific past-tense action or outcome (e.g., <c>registered</c>, <c>requested</c>).</description></item>
    /// </list>
    /// </remarks>
    public static class Topics
    {
        public const string IdentityUserRegistered = "identity.user.registered";
        public const string IdentityUserOnboardingCompleted = "identity.user.onboarding-completed";

        public const string ActivityLearningActivityRequested = "activity.learning-activity.requested";
        public const string ActivityLearningActivityRequestedRetry = "activity.learning-activity-requested.retry";

        public const string ActivityLearningActivityGenerated = "activity.learning-activity.generated";
    }

    /// <summary>
    /// Centralized registry for Kafka consumer group identifiers.
    /// </summary>
    /// <remarks>
    /// Consumer groups must strictly adhere to the following architectural naming format:
    /// <para><c>[consuming-service].[handler-purpose]-group</c></para>
    /// <list type="bullet">
    /// <item><description><b>consuming-service:</b> The specific microservice host running the background worker (e.g., <c>activity-service</c>).</description></item>
    /// <item><description><b>handler-purpose:</b> The distinct domain responsibility executed by the consumer cluster (e.g., <c>user-onboarding</c>).</description></item>
    /// </list>
    /// </remarks>
    public static class ConsumerGroups
    {
        public const string ActivityServiceQuizActivityGenerator = "activity-service.learning-activity-generator-group";
        public const string ActivityServiceUserOnboarding = "activity-service.user-onboarding-group";
        public const string ActivityServiceQuizActivityGeneratorRetry = "activity-service.quiz-activity-generator.retry";
    }
}