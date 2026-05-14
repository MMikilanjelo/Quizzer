using Cysharp.Threading.Tasks;
using Source.Features.Onboarding.Mediator;
using Source.Features.Onboarding.TellUsYourGoal.Components;
using Source.Features.Onboarding.TellUsYourGoal.ViewModels;
using Source.Features.Onboarding.TellUsYourInterests.Components;
using Source.Features.Onboarding.TellUsYourInterests.ViewModels;
using Source.Features.Onboarding.TellUsYourProficiency.Components;
using Source.Features.Onboarding.TellUsYourProficiency.ViewModels;
using Source.Shared;
using Source.Shared.Extensions;
using Source.Shared.Services;
using UnityEngine.UIElements;

namespace Source.Features.Onboarding.Factory
{
    internal class OnboardingUIFactory : IOnboardingUIFactory
    {
        private readonly IAssetProviderService _assetProviderService;

        public OnboardingUIFactory(IAssetProviderService assetProviderService) =>
            _assetProviderService = assetProviderService;

        public UniTask<Result<TellUsYourGoalScreen>> CreateTellUsYourGoalScreen(ITellUsYourGoalScreenViewModel viewModel)
        {
            var templateContainer = new TemplateContainer();

            return UniTask.FromResult(Result<TellUsYourGoalScreen>.Success(new TellUsYourGoalScreen(templateContainer, viewModel , _assetProviderService.Icons)));
        }

        public UniTask<Result<TellUsYourInterestsScreen>> CreateTellUsYourInterestsScreen(ITellUsYourInterestsScreenViewModel viewModel)
        {
            var templateContainer = new TemplateContainer();

            return UniTask.FromResult(Result<TellUsYourInterestsScreen>.Success(new TellUsYourInterestsScreen(templateContainer, viewModel, _assetProviderService.Icons)));
        }

        public UniTask<Result<TellUsYourProficiencyScreen>> CreateTellUsYourProficiencyScreen(ITellUsYourProficiencyScreenViewModel viewModel)
        {
            var templateContainer = new TemplateContainer();

            return UniTask.FromResult(Result<TellUsYourProficiencyScreen>.Success(new TellUsYourProficiencyScreen(templateContainer, viewModel, _assetProviderService.Icons)));
        }
    }
}