using System;
using Source.App.Bootstrap;
using Source.App.Mediator;
using Source.App.StateMachine;
using Source.App.StateMachine.States.BootstrapState;
using Source.App.StateMachine.States.MainState;
using Source.App.StateMachine.States.MainState.StateMachine;
using Source.App.StateMachine.States.MainState.StateMachine.States;
using Source.Features.Auth;
using Source.Features.FAB;
using Source.Features.LoadingOverlays;
using Source.Features.Onboarding;
using Source.Features.Quizzes;
using Source.Features.TabBar;
using Source.Features.TechnicalDialogs;
using Source.Shared.Persistence;
using Source.Shared.Persistence.DataBase;
using Source.Shared.Persistence.DataBase.Converters;
using Source.Shared.Services;
using Source.Shared.UIStack.Mediator;
using VContainer;
using VContainer.Unity;

namespace Source.App
{
    public class DependencyInjection : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            DontDestroyOnLoad(gameObject);
            builder.Register<UIStackMediator>(Lifetime.Singleton).AsImplementedInterfaces();

            builder.RegisterTechnicalDialogsFeature();
            builder.RegisterLoadingOverlaysFeature();
            builder.RegisterAuthFeature();
            builder.RegisterOnboardingFeature();
            builder.RegisterTabBarFeature();
            builder.RegisterFabFeature();
            builder.RegisterQuizzesFeature();

            builder.Register<IAssetProviderService, AssetProviderService>(Lifetime.Singleton);
            builder.Register<ILoggingService, LoggingService>(Lifetime.Singleton);
            builder.Register<IWebApiService, WebApiService>(Lifetime.Singleton);
            builder.Register<ISerializationService, SerializationService>(Lifetime.Singleton);
            builder.Register<IEncryptionService, AesEncryptionService>(Lifetime.Singleton);
            builder.Register<UltraLiteDocumentStore>(Lifetime.Singleton);

            builder.Register<IIdConverter<int>, IntIdConverter>(Lifetime.Singleton);
            builder.Register<IIdConverter<string>, StringIdConverter>(Lifetime.Singleton);
            builder.Register<IIdConverter<Guid>, GuidIdConverter>(Lifetime.Singleton);
            builder.Register(typeof(IRepository<,>), typeof(UltraLiteRepository<,>), Lifetime.Singleton);

            RegisterApplicationStateMachine(builder);
            RegisterGlobalStateMachine(builder);

            builder.RegisterEntryPoint<Bootstrapper>();
        }

        private static void RegisterApplicationStateMachine(IContainerBuilder builder)
        {
            builder.Register<ApplicationStateMachine>(Lifetime.Singleton);
            builder.Register<SignInState>(Lifetime.Singleton);
            builder.Register<TellUsYourInterestsState>(Lifetime.Singleton);
            builder.Register<TellUsYourProficiencyLevelState>(Lifetime.Singleton);
            builder.Register<TellUsYourGoalState>(Lifetime.Singleton);
            builder.Register<HomeState>(Lifetime.Singleton);
            builder.Register<ProfileState>(Lifetime.Singleton);
            builder.Register<MyQuizzesState>(Lifetime.Singleton);
            builder.Register<CreateQuizState>(Lifetime.Singleton);
            builder.Register<ActiveQuizState>(Lifetime.Singleton);
            builder.Register<FinishedQuizState>(Lifetime.Singleton);
            builder.Register<IAppMediator, AppMediator>(Lifetime.Singleton);
        }

        private static void RegisterGlobalStateMachine(IContainerBuilder builder)
        {
            builder.Register<GlobalStateMachine>(Lifetime.Singleton);
            builder.Register<BootstrapState>(Lifetime.Singleton);
            builder.Register<MainState>(Lifetime.Singleton);
        }
    }
}