using Source.Shared.Reactive.Commands;
using Source.Shared.Reactive.List;

namespace Source.Features.Onboarding.TellUsYourGoal.ViewModels
{
    public interface ITellUsYourGoalScreenViewModel
    {
        ICommand ContinueCommand { get; }
        ICommand<YourGoalItemViewModel> SelectGoalCommand { get; }
        IReadOnlyReactiveList<YourGoalItemViewModel> Goals { get; }
        int CurrentStep { get; }
        int TotalSteps { get; }
    }
}