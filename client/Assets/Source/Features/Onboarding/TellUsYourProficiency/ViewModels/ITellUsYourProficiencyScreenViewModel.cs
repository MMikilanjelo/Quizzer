using Source.Shared.Reactive.Commands;
using Source.Shared.Reactive.List;
using Source.Shared.Services;

namespace Source.Features.Onboarding.TellUsYourProficiency.ViewModels
{
    public interface ITellUsYourProficiencyScreenViewModel
    {
        IReadOnlyReactiveList<ProficiencyItemViewModel> Proficiencies { get; }
        ICommand<ProficiencyItemViewModel> SelectProficiencyCommand { get; }
        ICommand ContinueCommand { get; }
        ICommand GoBackCommand { get; }
        int CurrentStep { get; }
        int TotalSteps { get; }
    }
}