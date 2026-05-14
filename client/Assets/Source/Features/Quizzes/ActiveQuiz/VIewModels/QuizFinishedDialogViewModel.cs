using Source.Shared.Reactive.Commands;

namespace Source.Features.Quizzes.ActiveQuiz.VIewModels
{
    public class QuizFinishedDialogViewModel
    {
        public ICommand PrimaryActionCommand { get; }
        public ICommand SecondaryActionCommand { get; }

        public QuizFinishedDialogViewModel(
            ICommand primaryActionCommand,
            ICommand secondaryActionCommand
        )
        {
            PrimaryActionCommand = primaryActionCommand;
            SecondaryActionCommand = secondaryActionCommand;
        }
    }
}