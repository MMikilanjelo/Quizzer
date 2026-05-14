using Source.Features.TabBar.Mediator;
using VContainer;

namespace Source.Features.TabBar
{
    public static class DependencyInjection
    {
        public static IContainerBuilder RegisterTabBarFeature(this IContainerBuilder builder)
        {
            builder.Register<ITabBarMediator, TabBarMediator>(Lifetime.Singleton);

            return builder;
        }
    }
}