using Source.Features.Quizzes.CreateQuiz.Components;
using Source.Features.Quizzes.CreateQuiz.Models;
using Source.Shared.Reactive.Commands;
using Source.Shared.Reactive.Events;
using Source.Shared.Reactive.List;

namespace Source.Features.Quizzes.CreateQuiz.ViewModels
{
    public interface ICreateQuizScreenViewModel
    {
        SelectNumberOfQuestionViewModel NumberOfQuestion { get; }
        ICommand<QuizCreationMode> ChangeModeCommand { get; }
        ICommand GoBackCommand { get; }
        ICommand CreateQuizCommand { get; }
        ICommand<SelectDifficultyLevelItemViewModel> SelectDifficultyLevelCommand { get; }
        ICommand<SelectDomainItemViewModel> SelectDomainCommand { get; }
        IReactiveProperty<QuizCreationMode> CurrentMode { get; }
        IReadOnlyReactiveList<SelectDifficultyLevelItemViewModel> DifficultyLevels { get; }
        IReadOnlyReactiveList<SelectDomainItemViewModel> Domains { get; }
    }
}