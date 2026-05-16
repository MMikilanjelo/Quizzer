using Source.Shared.Reactive.Commands;
using Source.Shared.Reactive.Events;
using Source.Shared.Reactive.List;

namespace Source.Features.Quizzes.MyQuizzes.ViewModels
{
    public interface IMyQuizzesScreenViewModel
    {
        ICommand FetchQuizzesCommand { get; }
        IReadOnlyReactiveProperty<bool> IsLoading { get; }
        ICommand<QuizzesStateFilterItemViewModel> SelectStateFilter { get; }
        IReadOnlyReactiveList<QuizzesStateFilterItemViewModel> States { get; }
        IReadOnlyReactiveList<QuizItemViewModel> Quizzes { get; }
        NoQuizzesViewModel EmptyState { get; }
    }
}