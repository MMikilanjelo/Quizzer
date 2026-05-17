using System;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Source.App.Mediator;
using Source.Features.FAB.Mediator;
using Source.Features.Quizzes.CreateQuiz.Models;
using Source.Features.Quizzes.CreateQuiz.UseCases;
using Source.Features.Quizzes.CreateQuiz.ViewModels;
using Source.Features.Quizzes.Mediator;
using Source.Features.TabBar.Mediator;
using Source.Shared;
using Source.Shared.Extensions;
using Source.Shared.Reactive;
using Source.Shared.Reactive.Commands;
using Source.Shared.Reactive.Disposables;
using Source.Shared.Reactive.Events;
using Source.Shared.Reactive.List;
using Source.Shared.Reactive.SelectableList;
using Source.Shared.StateMachine.States;
using Source.Shared.UIStack.Mediator;

namespace Source.App.StateMachine.States.MainState.StateMachine.States
{
    public class CreateQuizStatePayload
    {
        public Action GoBackAction { get; set; }
    }

    public class CreateQuizState :
        ApplicationState,
        IPayloadState<CreateQuizStatePayload>,
        IExitState,
        ICreateQuizScreenViewModel
    {
        public IReactiveProperty<QuizCreationMode> CurrentMode => _creationMode;
        public SelectNumberOfQuestionViewModel NumberOfQuestion { get; } = new();
        public ICommand<QuizCreationMode> ChangeModeCommand { get; private set; }
        public ICommand GoBackCommand { get; private set; }
        public ICommand CreateQuizCommand { get; private set; }
        public ICommand<SelectDifficultyLevelItemViewModel> SelectDifficultyLevelCommand => _difficultyLevels.SelectCommand;
        public IReadOnlyReactiveList<SelectDifficultyLevelItemViewModel> DifficultyLevels => _difficultyLevels.Items;
        public ICommand<SelectDomainItemViewModel> SelectDomainCommand => _domains.SelectCommand;
        public IReadOnlyReactiveList<SelectDomainItemViewModel> Domains => _domains.Items;

        private readonly SelectableList<SelectDifficultyLevelItemViewModel> _difficultyLevels = SelectableList<SelectDifficultyLevelItemViewModel>.Exclusive();
        private readonly SelectableList<SelectDomainItemViewModel> _domains = SelectableList<SelectDomainItemViewModel>.Exclusive();
        private readonly ReactiveProperty<QuizCreationMode> _creationMode = new(QuizCreationMode.Smart);

        private readonly IUseCase<LoadQuizConfiguration.Response> _fetchQuizConfiguration;
        private readonly IUseCase<SmartScheduleQuiz.Response> _smartScheduleQuiz;
        private readonly IUseCase<ManualScheduleQuiz.Request, ManualScheduleQuiz.Response> _manualScheduleQuiz;

        private readonly ITabBarMediator _tabBarMediator;
        private readonly IQuizzesMediator _quizzesMediator;
        private readonly IUIStackMediator _uiStackMediator;
        private readonly IAppMediator _appMediator;
        private readonly IQuizCreationStore _quizCreationStore;

        private readonly ReactiveProperty<bool> _isCreationAvailable = new(false);

        private CancellationTokenSource _cancellationTokenSource;

        private CompositeDisposable _bindings;
        private CreateQuizStatePayload _payload;

        public CreateQuizState(
            ITabBarMediator tabBarMediator,
            IQuizzesMediator quizzesMediator,
            IUIStackMediator uiStackMediator,
            IAppMediator appMediator,
            IQuizCreationStore quizCreationStore,
            IUseCase<LoadQuizConfiguration.Response> fetchQuizConfiguration,
            IUseCase<SmartScheduleQuiz.Response> smartScheduleQuiz,
            IUseCase<ManualScheduleQuiz.Request, ManualScheduleQuiz.Response> manualScheduleQuiz
        )
        {
            _tabBarMediator = tabBarMediator;
            _quizzesMediator = quizzesMediator;
            _uiStackMediator = uiStackMediator;
            _appMediator = appMediator;
            _quizCreationStore = quizCreationStore;

            _fetchQuizConfiguration = fetchQuizConfiguration;
            _smartScheduleQuiz = smartScheduleQuiz;
            _manualScheduleQuiz = manualScheduleQuiz;
        }

        public void Enter(CreateQuizStatePayload payload)
        {
            _payload = payload;

            _cancellationTokenSource = new CancellationTokenSource();
            _bindings = new CompositeDisposable();

            _tabBarMediator.Hide();

            GoBackCommand = SyncCommand.Create(() => { _payload.GoBackAction?.Invoke(); });
            CreateQuizCommand = AsyncCommand
                .Create(CreateQuiz)
                .WithExecutionRule(_isCreationAvailable);

            ChangeModeCommand = SyncCommand<QuizCreationMode>.Create(mode =>
            {
                _creationMode.Value = mode;
                _quizCreationStore.Get().Mode = mode;
            });

            NumberOfQuestion.Value
                .Subscribe(OnQuizCountChanged)
                .AddTo(_bindings);

            _creationMode
                .Subscribe(_ => EvaluateCreationAvailability())
                .AddTo(_bindings);

            _domains.ItemSelectionChanged
                .Subscribe(viewModel =>
                {
                    OnDomainSelectionChanged(viewModel);
                    EvaluateCreationAvailability();
                })
                .AddTo(_bindings);

            _difficultyLevels.ItemSelectionChanged
                .Subscribe(viewModel =>
                {
                    OnDifficultyLevelSelectionChanged(viewModel);
                    EvaluateCreationAvailability();
                })
                .AddTo(_bindings);

            _quizzesMediator.CreateCreateQuizScreen(this);

            EvaluateCreationAvailability();

            FetchQuizConfiguration().Forget();
        }

        private UniTask FetchQuizConfiguration()
        {
            return _fetchQuizConfiguration
                .Execute(_cancellationTokenSource.Token)
                .Tap(_ =>
                {
                    _domains.Set(_quizCreationStore
                        .Get().Configuration.Domains
                        .Select(d => new SelectDomainItemViewModel(d))
                    );

                    _difficultyLevels.Set(_quizCreationStore
                        .Get().Configuration.Difficulties
                        .Select(d => new SelectDifficultyLevelItemViewModel(d))
                    );
                })
                .CatchAll(error => _appMediator.TechnicalErrorOccured.Invoke(error));
        }

        private UniTask CreateQuiz()
        {
            if (_quizCreationStore.Get().Mode == QuizCreationMode.Manual)
            {
                var quizCreationModel = _quizCreationStore.Get();

                return _manualScheduleQuiz
                    .Execute(
                        new ManualScheduleQuiz.Request
                        {
                            DomainId = quizCreationModel.SelectedDomain,
                            QuestionCount = quizCreationModel.RequestedQuestions,
                            DifficultyLevel = quizCreationModel.SelectedDifficulty
                        },
                        _cancellationTokenSource.Token
                    )
                    .Tap(_ => _payload.GoBackAction?.Invoke())
                    .CatchAll(error => _appMediator.TechnicalErrorOccured.Invoke(error));
            }

            return _smartScheduleQuiz
                .Execute(_cancellationTokenSource.Token)
                .Tap(_ => _payload.GoBackAction?.Invoke())
                .CatchAll(error => _appMediator.TechnicalErrorOccured.Invoke(error));
        }

        public void Exit()
        {
            _payload = null;
            GoBackCommand.Dispose();
            CreateQuizCommand.Dispose();

            _uiStackMediator.PopAllScreens();

            _difficultyLevels.Dispose();
            _domains.Dispose();

            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;

            _bindings.Dispose();
            _bindings = null;

            _quizCreationStore.Clear();
        }

        private void OnDifficultyLevelSelectionChanged(SelectDifficultyLevelItemViewModel viewModel)
        {
            if (viewModel.IsSelected.Value)
            {
                _quizCreationStore.Get().SelectedDifficulty = viewModel.Name;
            }
        }

        private void OnDomainSelectionChanged(SelectDomainItemViewModel viewModel)
        {
            if (viewModel.IsSelected.Value)
            {
                _quizCreationStore.Get().SelectedDomain = viewModel.Name;
            }
        }

        private void OnQuizCountChanged(int count)
        {
            var quizCreationModel = _quizCreationStore.Get();

            quizCreationModel.RequestedQuestions = count;
        }

        private void EvaluateCreationAvailability()
        {
            var mode = _creationMode.Value;
            var isDomainValid = _domains.IsValid.Value;
            var isDifficultyValid = _difficultyLevels.IsValid.Value;

            if (mode == QuizCreationMode.Smart)
            {
                _isCreationAvailable.Value = true;
            }
            else
            {
                _isCreationAvailable.Value = isDomainValid && isDifficultyValid;
            }
        }
    }
}