using Cysharp.Threading.Tasks;
using Source.Features.Quizzes.ActiveQuiz.VIewModels;
using Source.Features.Quizzes.CreateQuiz.ViewModels;
using Source.Features.Quizzes.Factory;
using Source.Features.Quizzes.FinishedQuiz.ViewModels;
using Source.Features.Quizzes.MyQuizzes.ViewModels;
using Source.Shared;
using Source.Shared.Extensions;
using Source.Shared.UIStack.Mediator;

namespace Source.Features.Quizzes.Mediator
{
    internal class QuizzesMediator : IQuizzesMediator
    {
        private readonly IQuizzesUIFactory _factory;
        private readonly IUIStackMediator _stackMediator;

        public QuizzesMediator(IQuizzesUIFactory factory, IUIStackMediator stackMediator)
        {
            _factory = factory;
            _stackMediator = stackMediator;
        }

        public async UniTask<Result> CreateMyQuizzesScreen(IMyQuizzesScreenViewModel viewModel)
        {
            return await _factory
                .CreateMyQuizzesScreen(viewModel)
                .Tap(view => _stackMediator.Push(view));
        }

        public async UniTask<Result> CreateCreateQuizScreen(ICreateQuizScreenViewModel viewModel)
        {
            return await _factory
                .CreateCreateQuizScreen(viewModel)
                .Tap(view => _stackMediator.Push(view));
        }

        public async UniTask<Result> CreateActiveQuizScreen(IActiveQuizScreenViewModel viewModel)
        {
            return await _factory
                .CreateActiveQuizScreen(viewModel)
                .Tap(view => _stackMediator.Push(view));
        }

        public async UniTask<Result> CreateFinishedQuizScreen(IFinishedQuizScreenViewModel viewModel)
        {
            return await _factory
                .CreateFinishedQuizScreen(viewModel)
                .Tap(view => _stackMediator.Push(view));
        }

        public async UniTask<Result> CreateQuizFinishedDialog(QuizFinishedDialogViewModel viewModel)
        {
            return await _factory
                .CreateQuizFinishedDialog(viewModel)
                .Tap(view => _stackMediator.Push(view));
        }
    }
}