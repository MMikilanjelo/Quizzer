using Source.Features.Onboarding.Factory;
using Source.Features.Onboarding.Mediator;
using Source.Features.Onboarding.Models;
using Source.Features.Onboarding.UseCases;
using Source.Shared;
using VContainer;

namespace Source.Features.Onboarding
{
    public static class DependencyInjection
    {
        public static IContainerBuilder RegisterOnboardingFeature(this IContainerBuilder builder)
        {
            builder.Register<IUseCase<LoadOnboardingQuestionnaire.Response>, LoadOnboardingQuestionnaire.UseCase>(Lifetime.Singleton);
            builder.Register<IUseCase<SubmitOnboarding.Request, SubmitOnboarding.Response>, SubmitOnboarding.UseCase>(Lifetime.Singleton);

            builder.Register<IOnboardingMediator, OnboardingMediator>(Lifetime.Singleton);

            builder.Register<IOnboardingUIFactory, OnboardingUIFactory>(Lifetime.Singleton);

            builder.Register<IOnboardingStore, OnboardingStore>(Lifetime.Singleton);

            return builder;
        }
    }
}