using System.Threading;
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
using Source.Shared.Extensions;
using Source.Shared.Services;
using UnityEngine.UIElements;

namespace Source.Features.Quizzes.Factory
{
    internal class QuizzesUIFactory : IQuizzesUIFactory
    {
        private readonly IAssetProviderService _assetProviderService;

        public QuizzesUIFactory(IAssetProviderService assetProviderService) =>
            _assetProviderService = assetProviderService;

        public async UniTask<Result<MyQuizzesScreen>> CreateMyQuizzesScreen(IMyQuizzesScreenViewModel viewModel)
        {
            return await _assetProviderService
                .LoadAsset<StyleSheet>("MyQuizzesScreen", "Global", CancellationToken.None)
                .Bind(result =>
                {
                    var templateContainer = new TemplateContainer();

                    templateContainer.styleSheets.Add(result);

                    return Result<MyQuizzesScreen>.Success(new MyQuizzesScreen(templateContainer, viewModel, _assetProviderService.Icons));
                });
        }

        public async UniTask<Result<CreateQuizScreen>> CreateCreateQuizScreen(ICreateQuizScreenViewModel viewModel)
        {
            return await _assetProviderService
                .LoadAsset<StyleSheet>("CreateQuizScreen", "Global", CancellationToken.None)
                .Bind(result =>
                {
                    var templateContainer = new TemplateContainer();

                    templateContainer.styleSheets.Add(result);

                    return UniTask.FromResult(Result<CreateQuizScreen>.Success(new CreateQuizScreen(templateContainer, viewModel, _assetProviderService.Icons)));
                });
        }

        public async UniTask<Result<ActiveQuizScreen>> CreateActiveQuizScreen(IActiveQuizScreenViewModel viewModel)
        {
            return await _assetProviderService
                .LoadAsset<StyleSheet>("QuizQuestion", "Global", CancellationToken.None)
                .Bind(result =>
                {
                    var templateContainer = new TemplateContainer();

                    templateContainer.styleSheets.Add(result);

                    return UniTask.FromResult(Result<ActiveQuizScreen>.Success(new ActiveQuizScreen(templateContainer, viewModel, _assetProviderService.Icons)));
                });
        }

        public async UniTask<Result<FinishedQuizScreen>> CreateFinishedQuizScreen(IFinishedQuizScreenViewModel viewModel)
        {
            return await _assetProviderService
                .LoadAsset<StyleSheet>("FinishedQuizScreen", "Global", CancellationToken.None)
                .Bind(result =>
                {
                    var templateContainer = new TemplateContainer();

                    templateContainer.styleSheets.Add(result);

                    return UniTask.FromResult(Result<FinishedQuizScreen>.Success(new FinishedQuizScreen(templateContainer, viewModel, _assetProviderService.Icons)));
                });
        }

        public UniTask<Result<QuizFinishedDialog>> CreateQuizFinishedDialog(QuizFinishedDialogViewModel viewModel)
        {
            var templateContainer = new TemplateContainer();

            return UniTask.FromResult(Result<QuizFinishedDialog>.Success(new QuizFinishedDialog(templateContainer, viewModel, _assetProviderService.Icons)));
        }
    }
}