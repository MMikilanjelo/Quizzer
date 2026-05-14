using Cysharp.Threading.Tasks;
using Source.Features.Onboarding.Factory;
using Source.Features.Onboarding.TellUsYourGoal.ViewModels;
using Source.Features.Onboarding.TellUsYourInterests.ViewModels;
using Source.Features.Onboarding.TellUsYourProficiency.ViewModels;
using Source.Shared;
using Source.Shared.Components.Screens;
using Source.Shared.Extensions;
using Source.Shared.UIStack.Mediator;

namespace Source.Features.Onboarding.Mediator
{
    internal class OnboardingMediator : IOnboardingMediator
    {
        private readonly IOnboardingUIFactory _factory;
        private readonly IUIStackMediator _stackMediator;

        public OnboardingMediator(IOnboardingUIFactory factory, IUIStackMediator stackMediator)
        {
            _factory = factory;
            _stackMediator = stackMediator;
        }

        public async UniTask<Result> CreateTellUsYourGoalScreen(ITellUsYourGoalScreenViewModel viewModel)
        {
            return await _factory
                .CreateTellUsYourGoalScreen(viewModel)
                .Tap(view => _stackMediator.Push(view));
        }

        public async UniTask<Result> CreateTellUsYourInterestsScreen(ITellUsYourInterestsScreenViewModel viewModel)
        {
            return await _factory
                .CreateTellUsYourInterestsScreen(viewModel)
                .Tap(view => _stackMediator.Push(view));
        }

        public async UniTask<Result> CreateTellUsYourProficiencyScreen(ITellUsYourProficiencyScreenViewModel viewModel)
        {
            return await _factory
                .CreateTellUsYourProficiencyScreen(viewModel)
                .Tap(view => _stackMediator.Push(view));
        }
    }
}