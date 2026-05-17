using Source.Shared.Reactive.Events;
using Source.Shared.Reactive.List;

namespace Source.Features.MyProfile.ViewModels
{
    public interface IMyProfileScreenViewModel
    {
        IReadOnlyReactiveProperty<bool> IsLoading { get; }
        IReadOnlyReactiveProperty<float> AverageScore { get; }
        IReadOnlyReactiveProperty<int> TotalQuizzes { get; }
        IReadOnlyReactiveProperty<int> PerfectQuizzes { get; }
        IReadOnlyReactiveProperty<int> StreakDays { get; }
        IReadOnlyReactiveList<ConceptMasteryLevelViewModel> TopStrengths { get; }
        IReadOnlyReactiveList<ConceptMasteryLevelViewModel> FocusAreas { get; }
    }
}