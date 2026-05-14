using Source.Shared.Reactive.Commands;
using Source.Shared.Reactive.List;
using Source.Shared.Services;

namespace Source.Features.Onboarding.TellUsYourInterests.ViewModels
{
    public interface ITellUsYourInterestsScreenViewModel
    {
        IReadOnlyReactiveList<YourInterestItemViewModel> Interests { get; }
        ICommand<YourInterestItemViewModel> SelectInterestCommand { get; }
        ICommand ContinueCommand { get; }
        ICommand GoBackCommand { get; }
        int CurrentStep { get; }
        int TotalSteps { get; }
    }
}