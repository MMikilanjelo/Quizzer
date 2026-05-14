using Source.Shared.Reactive.Commands;
using Source.Shared.Reactive.Events;
using Source.Shared.Reactive.List;

namespace Source.Features.Quizzes.ActiveQuiz.VIewModels
{
    public interface IActiveQuizScreenViewModel
    {
        ICommand<QuizOptionViewModel> SelectOptionCommand { get; }
        ICommand GoBackCommand { get; }
        ICommand ContinueCommand { get; }
        IReactiveProperty<float> Progress { get; }
        IReadOnlyReactiveList<string> Topics { get; }
        IReadOnlyReactiveProperty<string> QuestionText { get; }
        IReadOnlyReactiveList<QuizOptionViewModel> Options { get; }
        IReactiveProperty<string> QuizName { get; }
    }
}