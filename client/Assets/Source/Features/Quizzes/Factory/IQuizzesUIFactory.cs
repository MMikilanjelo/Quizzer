using Cysharp.Threading.Tasks;
using Source.Features.Quizzes.ActiveQuiz.Components;
using Source.Features.Quizzes.ActiveQuiz.VIewModels;
using Source.Features.Quizzes.CreateQuiz.Components;
using Source.Features.Quizzes.CreateQuiz.ViewModels;
using Source.Features.Quizzes.FinishedQuiz.Components;
using Source.Features.Quizzes.FinishedQuiz.ViewModels;
using Source.Features.Quizzes.MyQuizzes.Components;
using Source.Features.Quizzes.MyQuizzes.ViewModels;
using Source.Shared;

namespace Source.Features.Quizzes.Factory
{
    internal interface IQuizzesUIFactory
    {
        UniTask<Result<MyQuizzesScreen>> CreateMyQuizzesScreen(IMyQuizzesScreenViewModel viewModel);
        UniTask<Result<CreateQuizScreen>> CreateCreateQuizScreen(ICreateQuizScreenViewModel viewModel);
        UniTask<Result<ActiveQuizScreen>> CreateActiveQuizScreen(IActiveQuizScreenViewModel viewModel);
        UniTask<Result<FinishedQuizScreen>> CreateFinishedQuizScreen(IFinishedQuizScreenViewModel viewModel);
        UniTask<Result<QuizFinishedDialog>> CreateQuizFinishedDialog(QuizFinishedDialogViewModel viewModel);
    }
}