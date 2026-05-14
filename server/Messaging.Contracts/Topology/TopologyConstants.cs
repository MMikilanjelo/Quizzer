namespace Messaging.Contracts.Topology;

public static class Topology
{
    public static class Topics
    {
        // Format: [context].[aggregate].[event]
        public const string IdentityUserRegistered = "identity.user.registered";
        public const string ActivityLearningActivityRequested = "activity.learning-activity.requested";
        public const string ActivityLearningActivityGenerated = "activity.learning-activity.generated";
    }

    public static class ConsumerGroups
    {
        // Format: [consuming-service].[handler-purpose]-group
        public const string ActivityServiceQuizActivityGenerator = "activity-service.learning-activity-generator-group";
    }
}