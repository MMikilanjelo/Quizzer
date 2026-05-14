using Cysharp.Threading.Tasks;
using Source.Features.Onboarding.TellUsYourGoal.ViewModels;
using Source.Features.Onboarding.TellUsYourInterests.ViewModels;
using Source.Features.Onboarding.TellUsYourProficiency.ViewModels;
using Source.Shared;
using Source.Shared.Components.Screens;

namespace Source.Features.Onboarding.Mediator
{
    public interface IOnboardingMediator
    {
        UniTask<Result> CreateTellUsYourGoalScreen(ITellUsYourGoalScreenViewModel viewModel);
        UniTask<Result> CreateTellUsYourInterestsScreen(ITellUsYourInterestsScreenViewModel viewModel);
        UniTask<Result> CreateTellUsYourProficiencyScreen(ITellUsYourProficiencyScreenViewModel viewModel);
    }
}