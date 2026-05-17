using System;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Source.App.Mediator;
using Source.Features.Quizzes.FinishedQuiz.Components;
using Source.Features.Quizzes.FinishedQuiz.Models;
using Source.Features.Quizzes.FinishedQuiz.UseCases;
using Source.Features.Quizzes.FinishedQuiz.ViewModels;
using Source.Features.Quizzes.Mediator;
using Source.Features.TabBar.Mediator;
using Source.Shared;
using Source.Shared.Extensions;
using Source.Shared.Icons;
using Source.Shared.Reactive.Commands;
using Source.Shared.Reactive.Events;
using Source.Shared.Reactive.List;
using Source.Shared.StateMachine.States;
using Source.Shared.UIStack.Mediator;
using UnityEngine;

namespace Source.App.StateMachine.States.MainState.StateMachine.States
{
    public class FinishedQuizStatePayload
    {
        public string QuizId { get; set; }
        public Action GoBackAction { get; set; }
    }

    public class FinishedQuizState :
        ApplicationState,
        IPayloadState<FinishedQuizStatePayload>,
        IExitState,
        IFinishedQuizScreenViewModel
    {
        public IReactiveProperty<bool> IsLoading => _isLoading;
        public IReadOnlyReactiveProperty<string> QuizName => _quizName;
        public ICommand GoBackCommand { get; private set; }
        public IReadOnlyReactiveList<KnowledgeAreaItemViewModel> KnowledgeAreas => _knowledgeAreas;
        public PerformanceViewModel Performance { get; } = new();

        private readonly ReactiveList<KnowledgeAreaItemViewModel> _knowledgeAreas = new();
        private readonly ReactiveProperty<string> _quizName = new(string.Empty);
        private readonly ReactiveProperty<bool> _isLoading = new(false);

        private readonly IQuizzesMediator _quizzesMediator;
        private readonly IUIStackMediator _uiStackMediator;
        private readonly ITabBarMediator _tabBarMediator;
        private readonly IAppMediator _appMediator;
        private readonly IUseCase<FetchQuizAnalytics.Request, FetchQuizAnalytics.Response> _fetchQuizAnalyticsUseCase;

        private CancellationTokenSource _cancellationTokenSource;
        private FinishedQuizStatePayload _payload;

        public FinishedQuizState(
            IQuizzesMediator quizzesMediator,
            IUIStackMediator uiStackMediator,
            ITabBarMediator tabBarMediator,
            IAppMediator appMediator,
            IUseCase<FetchQuizAnalytics.Request, FetchQuizAnalytics.Response> fetchQuizAnalyticsUseCase
        )
        {
            _quizzesMediator = quizzesMediator;
            _uiStackMediator = uiStackMediator;
            _tabBarMediator = tabBarMediator;
            _appMediator = appMediator;
            _fetchQuizAnalyticsUseCase = fetchQuizAnalyticsUseCase;
        }

        public void Enter(FinishedQuizStatePayload payload)
        {
            _payload = payload;

            _tabBarMediator.Hide();

            GoBackCommand = SyncCommand.Create(() => { _payload.GoBackAction?.Invoke(); });

            _quizzesMediator.CreateFinishedQuizScreen(this);

            FetchAnalytics(payload.QuizId).Forget();
        }

        private UniTask FetchAnalytics(string id)
        {
            _isLoading.Value = true;

            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = new CancellationTokenSource();

            return _fetchQuizAnalyticsUseCase
                .Execute(
                    new FetchQuizAnalytics.Request { Id = id },
                    _cancellationTokenSource.Token
                )
                .Tap(response => { BindAnalyticsData(response.Analytics); })
                .CatchAll(error => _appMediator.TechnicalErrorOccured.Invoke(error))
                .Finally(() => _isLoading.Value = false);
        }

        private void BindAnalyticsData(QuizAnalyticsModel data)
        {
            _quizName.Value = data.Name;

            var total = data.AnsweredCount > 0 ? data.AnsweredCount : 1f;
            Performance.CorrectCount.Value = data.CorrectCount;
            Performance.IncorrectCount.Value = data.IncorrectCount;
            Performance.CorrectNormalized.Value = data.CorrectCount / total;
            Performance.IncorrectNormalized.Value = data.IncorrectCount / total;

            _knowledgeAreas.Clear();

            var mappedAreas = data.MasteryChanges.Select(m =>
            {
                var start = Mathf.RoundToInt((float)m.StartingMastery * 100);
                var end = Mathf.RoundToInt((float)m.EndingMastery * 100);
                var delta = end - start;

                var needsReview = delta < 0;

                var statusText = delta switch
                {
                    < 0 => "Needs review",
                    > 0 => $"+{delta}% improvement",
                    _ => "No change"
                };

                return new KnowledgeAreaItemViewModel
                {
                    TopicName = m.TopicId.ToTitleCase(),
                    StartPercentage = start,
                    EndPercentage = end,
                    NeedsReview = needsReview,
                    StatusText = statusText,
                    Icon = needsReview ? Icons.Warning : Icons.ArrowUpRight
                };
            });

            _knowledgeAreas.AddRange(mappedAreas);
        }

        public void Exit()
        {
            _payload = null;

            _uiStackMediator.PopAllScreens();
            _uiStackMediator.PopAllDialogs();
            _isLoading.Dispose();

            GoBackCommand.Dispose();

            _knowledgeAreas.Dispose();
            _quizName.Dispose();

            Performance.CorrectCount.Dispose();
            Performance.IncorrectCount.Dispose();
            Performance.CorrectNormalized.Dispose();
            Performance.IncorrectNormalized.Dispose();

            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
        }
    }
}