using Cysharp.Threading.Tasks;
using Source.Features.Onboarding.Mediator;
using Source.Features.Onboarding.TellUsYourGoal.Components;
using Source.Features.Onboarding.TellUsYourGoal.ViewModels;
using Source.Features.Onboarding.TellUsYourInterests.Components;
using Source.Features.Onboarding.TellUsYourInterests.ViewModels;
using Source.Features.Onboarding.TellUsYourProficiency.Components;
using Source.Features.Onboarding.TellUsYourProficiency.ViewModels;
using Source.Shared;

namespace Source.Features.Onboarding.Factory
{
    internal interface IOnboardingUIFactory
    {
        UniTask<Result<TellUsYourGoalScreen>> CreateTellUsYourGoalScreen(ITellUsYourGoalScreenViewModel viewModel);
        UniTask<Result<TellUsYourInterestsScreen>> CreateTellUsYourInterestsScreen(ITellUsYourInterestsScreenViewModel viewModel);
        UniTask<Result<TellUsYourProficiencyScreen>> CreateTellUsYourProficiencyScreen(ITellUsYourProficiencyScreenViewModel viewModel);
    }
}