using Source.Features.TechnicalDialogs.Factory;
using Source.Features.TechnicalDialogs.Mediator;
using VContainer;

namespace Source.Features.TechnicalDialogs
{
    public static class DependencyInjection
    {
        public static IContainerBuilder RegisterTechnicalDialogsFeature(this IContainerBuilder builder)
        {
            builder.Register<ITechnicalDialogsMediator, TechnicalDialogsMediator>(Lifetime.Singleton);
            builder.Register<ITechnicalDialogsFactory, TechnicalDialogsFactory>(Lifetime.Singleton);

            return builder;
        }
    }
}