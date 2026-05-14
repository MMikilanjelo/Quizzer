using Cysharp.Threading.Tasks;
using Source.Features.Quizzes.ActiveQuiz.VIewModels;
using Source.Features.Quizzes.CreateQuiz.ViewModels;
using Source.Features.Quizzes.FinishedQuiz.ViewModels;
using Source.Features.Quizzes.MyQuizzes.ViewModels;
using Source.Shared;

namespace Source.Features.Quizzes.Mediator
{
    public interface IQuizzesMediator
    {
        UniTask<Result> CreateMyQuizzesScreen(IMyQuizzesScreenViewModel viewModel);
        UniTask<Result> CreateCreateQuizScreen(ICreateQuizScreenViewModel viewModel);
        UniTask<Result> CreateActiveQuizScreen(IActiveQuizScreenViewModel viewModel);
        UniTask<Result> CreateFinishedQuizScreen(IFinishedQuizScreenViewModel viewModel);
        UniTask<Result> CreateQuizFinishedDialog(QuizFinishedDialogViewModel viewModel);
    }
}