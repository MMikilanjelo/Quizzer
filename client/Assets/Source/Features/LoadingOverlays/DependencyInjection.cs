using Source.Features.LoadingOverlays.Factory;
using Source.Features.LoadingOverlays.Mediator;
using VContainer;

namespace Source.Features.LoadingOverlays
{
    public static class DependencyInjection
    {
        public static IContainerBuilder RegisterLoadingOverlaysFeature(this IContainerBuilder builder)
        {
            builder.Register<ILoadingOverlayMediator, LoadingOverlayMediator>(Lifetime.Singleton);
            builder.Register<ILoadingOverlayFactory, LoadingOverlayFactory>(Lifetime.Singleton);

            return builder;
        }
    }
}