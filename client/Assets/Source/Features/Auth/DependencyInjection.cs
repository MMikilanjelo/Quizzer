using Source.Features.Auth.Models;
using Source.Features.Auth.SigIn.Factory;
using Source.Features.Auth.SigIn.Mediator;
using Source.Features.Auth.SigIn.UseCases;
using Source.Features.Auth.UseCases;
using Source.Shared;
using Source.Shared.Components.Screens;
using Source.Shared.Services;
using VContainer;

namespace Source.Features.Auth
{
    public static class DependencyInjection
    {
        public static IContainerBuilder RegisterAuthFeature(this IContainerBuilder builder)
        {
            builder.Register<IUseCase<CreateGuestAccount.Response>, CreateGuestAccount.UseCase>(Lifetime.Singleton);
            builder.Register<IUseCase<LoginGuestAccount.Response>, LoginGuestAccount.UseCase>(Lifetime.Singleton);
            builder.Register<IUseCase<GetAccessToken.Response>, GetAccessToken.UseCase>(Lifetime.Singleton);
            builder.Register<IUseCase<RefreshAccessToken.Response>, RefreshAccessToken.UseCase>(Lifetime.Singleton);

            builder.Register<ISignInUIFactory, SignInUIFactory>(Lifetime.Singleton);
            
            builder.Register<ITokenRepository, TokenRepository>(Lifetime.Singleton);

            builder.Register<ISignInMediator, SignInScreenMediator>(Lifetime.Singleton);

            builder.Register<IAuthorizedWebApiService, AuthorizedWebApiService>(Lifetime.Singleton);

            return builder;
        }
    }
}