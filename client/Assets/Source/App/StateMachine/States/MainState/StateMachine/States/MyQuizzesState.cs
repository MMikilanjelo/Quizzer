using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Source.App.Mediator;
using Source.Features.FAB.Mediator;
using Source.Features.FAB.ViewModels;
using Source.Features.Quizzes.CreateQuiz.UseCases;
using Source.Features.Quizzes.CreateQuiz.ViewModels;
using Source.Features.Quizzes.Mediator;
using Source.Features.Quizzes.MyQuizzes.Models;
using Source.Features.Quizzes.MyQuizzes.UseCases;
using Source.Features.Quizzes.MyQuizzes.ViewModels;
using Source.Features.TabBar.Mediator;
using Source.Shared;
using Source.Shared.Extensions;
using Source.Shared.Icons;
using Source.Shared.Reactive;
using Source.Shared.Reactive.Commands;
using Source.Shared.Reactive.Disposables;
using Source.Shared.Reactive.Events;
using Source.Shared.Reactive.List;
using Source.Shared.Reactive.SelectableList;
using Source.Shared.StateMachine.States;
using Source.Shared.UIStack.Mediator;
using UnityEngine;

namespace Source.App.StateMachine.States.MainState.StateMachine.States
{
    public class MyQuizzesState :
        ApplicationState,
        IEnterState,
        IExitState,
        IMyQuizzesScreenViewModel
    {
        public IReadOnlyReactiveProperty<bool> IsLoading => _isLoading;
        public ICommand FetchQuizzesCommand { get; private set; }
        public ICommand<QuizItemViewModel> ContinueQuizCommand { get; private set; }
        public ICommand<QuizzesStateFilterItemViewModel> SelectStateFilter => _stateFilters.SelectCommand;
        public IReadOnlyReactiveList<QuizzesStateFilterItemViewModel> States => _stateFilters.Items;
        public IReadOnlyReactiveList<QuizItemViewModel> Quizzes => _quizzes;
        public NoQuizzesViewModel EmptyState { get; private set; }
        private ICommand _createQuizCommand;

        private readonly IQuizzesMediator _quizzesMediator;
        private readonly IUIStackMediator _uiStackMediator;
        private readonly IFabMediator _fabMediator;
        private readonly ITabBarMediator _tabBarMediator;
        private readonly IAppMediator _appMediator;

        private readonly IUseCase<FetchQuizzes.Request, FetchQuizzes.Response> _fetchQuizzes;
        private readonly SelectableList<QuizzesStateFilterItemViewModel> _stateFilters = SelectableList<QuizzesStateFilterItemViewModel>.Exclusive();
        private readonly ReactiveList<QuizItemViewModel> _quizzes = new();
        private readonly ReactiveProperty<bool> _isLoading = new(false);

        private CancellationTokenSource _cancellationTokenSource;
        private CompositeDisposable _bindings;

        public MyQuizzesState(
            IQuizzesMediator quizzesMediator,
            IUIStackMediator uiStackMediator,
            IFabMediator fabMediator,
            ITabBarMediator tabBarMediator,
            IAppMediator appMediator,
            IUseCase<FetchQuizzes.Request, FetchQuizzes.Response> fetchQuizzes
        )
        {
            _quizzesMediator = quizzesMediator;
            _uiStackMediator = uiStackMediator;
            _fabMediator = fabMediator;
            _tabBarMediator = tabBarMediator;
            _appMediator = appMediator;

            _fetchQuizzes = fetchQuizzes;
        }

        public void Enter()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _bindings = new CompositeDisposable();
            EmptyState = new NoQuizzesViewModel(Icons.SealQuestion, string.Empty, string.Empty);

            _createQuizCommand = SyncCommand.Create(() => { });
            _createQuizCommand.Executed
                .Subscribe(_ =>
                {
                    StateMachine.Enter<CreateQuizState, CreateQuizStatePayload>(new CreateQuizStatePayload
                        {
                            GoBackAction = () => StateMachine.Enter<MyQuizzesState>()
                        }
                    );
                })
                .AddTo(_bindings);

            ContinueQuizCommand = SyncCommand<QuizItemViewModel>.Create(viewModel =>
            {
                if (viewModel.IsCompleted)
                {
                    StateMachine.Enter<FinishedQuizState, FinishedQuizStatePayload>(new FinishedQuizStatePayload()
                    {
                        QuizId = viewModel.Id,
                        GoBackAction = () => StateMachine.Enter<MyQuizzesState>()
                    });
                }
                else
                {
                    StateMachine.Enter<ActiveQuizState, ActiveQuizStatePayload>(new ActiveQuizStatePayload
                    {
                        QuizId = viewModel.Id,
                        GoBackAction = () => StateMachine.Enter<MyQuizzesState>()
                    });
                }
            });
            FetchQuizzesCommand = AsyncCommand.Create(() =>
            {
                var filter = _stateFilters.Items.First(i => i.IsSelected.Value).Model.Filter;

                return FetchQuizzes(filter);
            });

            _tabBarMediator.Show();
            _fabMediator.Set(new List<FabActionViewModel>
            {
                new("Create Quiz", Icons.SealQuestion, _createQuizCommand)
            });

            _stateFilters.ItemSelectionChanged
                .Subscribe(OnStateFilterSelectionChanged)
                .AddTo(_bindings);

            _stateFilters.Set(Enum.GetValues(typeof(QuizFilter))
                .Cast<QuizFilter>()
                .ToList()
                .Select(v => new QuizzesStateFilterItemViewModel(new QuizFilterModel(v), v == QuizFilter.All))
            );

            UpdateEmptyStateContent(_stateFilters.Items.First(i => i.IsSelected.Value).Model.Filter);

            _quizzesMediator.CreateMyQuizzesScreen(this);
        }

        private UniTask FetchQuizzes(QuizFilter filter)
        {
            _isLoading.Value = true;

            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = new CancellationTokenSource();

            return _fetchQuizzes
                .Execute(
                    new FetchQuizzes.Request
                    {
                        Page = 1,
                        PageSize = 10000,
                        Filter = filter
                    },
                    _cancellationTokenSource.Token
                )
                .Tap(response =>
                {
                    _quizzes.Clear();
                    _quizzes.AddRange(response.Quizzes.Items.Select(q => new QuizItemViewModel(q, ContinueQuizCommand)));
                })
                .CatchAll(error => _appMediator.TechnicalErrorOccured.Invoke(error))
                .Finally(() => _isLoading.Value = false);
        }

        private void OnStateFilterSelectionChanged(QuizzesStateFilterItemViewModel viewModel)
        {
            if (!viewModel.IsSelected.Value)
            {
                return;
            }

            UpdateEmptyStateContent(viewModel.Model.Filter);

            FetchQuizzes(viewModel.Model.Filter).Forget();
        }

        private void UpdateEmptyStateContent(QuizFilter filter)
        {
            switch (filter)
            {
                case QuizFilter.Pending:
                    EmptyState.Update(Icons.Coffee, "All caught up!", "You don't have any pending quizzes right now. Take a breather or start a new topic.");
                    break;
                case QuizFilter.Active:
                    EmptyState.Update(Icons.Lightbulb, "No active sessions", "You aren't currently taking any quizzes.");
                    break;
                case QuizFilter.Completed:
                    EmptyState.Update(Icons.Trophy, "Ready to earn your first badge?", "Quizzes you finish will show up here along with your performance analytics.");
                    break;
                case QuizFilter.All:
                default:
                    EmptyState.Update(Icons.MagicWand, "No quizzes found", "You haven't generated any quizzes yet. Tap the Create button to get started!");
                    break;
            }
        }

        public void Exit()
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;

            _isLoading.Value = false;

            _bindings.Dispose();
            _bindings = null;

            ContinueQuizCommand.Dispose();
            FetchQuizzesCommand.Dispose();
            _createQuizCommand.Dispose();

            _stateFilters.Dispose();
            EmptyState?.Dispose();

            _uiStackMediator.PopAllScreens();
            _fabMediator.Clear();
            _quizzes.Clear();
        }
    }
}