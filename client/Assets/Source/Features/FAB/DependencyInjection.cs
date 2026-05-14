using Source.Features.FAB.Mediator;
using VContainer;

namespace Source.Features.FAB
{
    public static class DependencyInjection
    {
        public static IContainerBuilder RegisterFabFeature(this IContainerBuilder builder)
        {
            builder.Register<IFabMediator, FabMediator>(Lifetime.Singleton);

            return builder;
        }
    }
}