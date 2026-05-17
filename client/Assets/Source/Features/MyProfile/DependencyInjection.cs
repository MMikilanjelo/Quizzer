using Source.Features.MyProfile.Factory;
using Source.Features.MyProfile.Mediator;
using Source.Features.MyProfile.UseCase;
using Source.Shared;
using VContainer;

namespace Source.Features.MyProfile
{
    public static class DependencyInjection
    {
        public static IContainerBuilder RegisterMyProfileFeature(this IContainerBuilder builder)
        {
            builder.Register<IMyProfileMediator, MyProfileMediator>(Lifetime.Singleton);
            builder.Register<IMyProfileFactory, MyProfileFactory>(Lifetime.Singleton);
            builder.Register<IUseCase<FetchMyProfile.Request, FetchMyProfile.Response>, FetchMyProfile.UseCase>(Lifetime.Singleton);
            return builder;
        }
    }
}