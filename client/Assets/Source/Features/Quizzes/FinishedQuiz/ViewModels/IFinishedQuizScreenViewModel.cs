using Source.Features.Quizzes.FinishedQuiz.Components;
using Source.Shared.Reactive.Commands;
using Source.Shared.Reactive.Events;
using Source.Shared.Reactive.List;

namespace Source.Features.Quizzes.FinishedQuiz.ViewModels
{
    public interface IFinishedQuizScreenViewModel
    {
        IReactiveProperty<bool> IsLoading { get; }
        IReadOnlyReactiveProperty<string> QuizName { get; }
        ICommand GoBackCommand { get; }
        IReadOnlyReactiveList<KnowledgeAreaItemViewModel> KnowledgeAreas { get; }
        PerformanceViewModel Performance { get; }
    }
}