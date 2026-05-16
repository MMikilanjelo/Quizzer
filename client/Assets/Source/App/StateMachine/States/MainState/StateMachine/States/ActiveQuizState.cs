using System;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Source.App.Mediator;
using Source.Features.Quizzes.ActiveQuiz.Models;
using Source.Features.Quizzes.ActiveQuiz.UseCases;
using Source.Features.Quizzes.ActiveQuiz.VIewModels;
using Source.Features.Quizzes.Mediator;
using Source.Features.TabBar.Mediator;
using Source.Shared;
using Source.Shared.Extensions;
using Source.Shared.Reactive.Commands;
using Source.Shared.Reactive.Events;
using Source.Shared.Reactive.List;
using Source.Shared.Reactive.SelectableList;
using Source.Shared.StateMachine.States;
using Source.Shared.UIStack.Mediator;

namespace Source.App.StateMachine.States.MainState.StateMachine.States
{
    public class ActiveQuizStatePayload
    {
        public string QuizId { get; set; }
        public Action GoBackAction { get; set; }
    }

    public class ActiveQuizState :
        ApplicationState,
        IExitState,
        IActiveQuizScreenViewModel,
        IPayloadState<ActiveQuizStatePayload>
    {
        public ICommand<QuizOptionViewModel> SelectOptionCommand => _options.SelectCommand;
        public ICommand GoBackCommand { get; private set; }
        public ICommand ContinueCommand { get; private set; }
        public IReactiveProperty<float> Progress => _progress;
        public IReadOnlyReactiveList<string> Topics => _topics;
        public IReadOnlyReactiveProperty<string> QuestionText => _questionText;
        public IReadOnlyReactiveList<QuizOptionViewModel> Options => _options.Items;
        public IReactiveProperty<string> QuizName => _quizName;

        private readonly SelectableList<QuizOptionViewModel> _options = SelectableList<QuizOptionViewModel>.Exclusive();
        private readonly ReactiveList<string> _topics = new();
        private readonly ReactiveProperty<float> _progress = new(0);
        private readonly ReactiveProperty<string> _quizName = new(string.Empty);
        private readonly ReactiveProperty<string> _questionText = new(string.Empty);

        private readonly IQuizzesMediator _quizzesMediator;
        private readonly IUIStackMediator _uiStackMediator;
        private readonly ITabBarMediator _tabBarMediator;
        private readonly IAppMediator _appMediator;
        private readonly IUseCase<FetchQuiz.Request, FetchQuiz.Response> _fetchQuizUseCase;
        private readonly IUseCase<SubmitAnswer.Request, SubmitAnswer.Response> _submitAnswerUseCase;
        private readonly IActiveQuizRepository _activeQuizRepository;

        private ICommand _goToFinishedQuizCommand;
        private ICommand _goBackCommand;

        private CancellationTokenSource _cancellationTokenSource;
        private ActiveQuizStatePayload _payload;

        public ActiveQuizState(
            IQuizzesMediator quizzesMediator,
            IUIStackMediator uiStackMediator,
            ITabBarMediator tabBarMediator,
            IAppMediator appMediator,
            IUseCase<FetchQuiz.Request, FetchQuiz.Response> fetchQuizUseCase,
            IUseCase<SubmitAnswer.Request, SubmitAnswer.Response> submitAnswerUseCase,
            IActiveQuizRepository activeQuizRepository
        )
        {
            _quizzesMediator = quizzesMediator;
            _uiStackMediator = uiStackMediator;
            _tabBarMediator = tabBarMediator;
            _appMediator = appMediator;
            _fetchQuizUseCase = fetchQuizUseCase;
            _submitAnswerUseCase = submitAnswerUseCase;
            _activeQuizRepository = activeQuizRepository;
        }

        public void Enter(ActiveQuizStatePayload payload)
        {
            _payload = payload;

            _tabBarMediator.Hide();

            GoBackCommand = SyncCommand.Create(() => { _payload.GoBackAction?.Invoke(); });
            ContinueCommand = AsyncCommand.Create(SubmitAnswer);
            _goToFinishedQuizCommand = SyncCommand.Create(() =>
            {
                StateMachine.Enter<FinishedQuizState, FinishedQuizStatePayload>(new FinishedQuizStatePayload
                {
                    QuizId = _activeQuizRepository.Get().Id,
                    GoBackAction = () => StateMachine.Enter<MyQuizzesState>()
                });
            });
            _goBackCommand = SyncCommand.Create(() => _payload.GoBackAction?.Invoke());

            _quizzesMediator.CreateActiveQuizScreen(this);

            FetchQuiz(payload.QuizId).Forget();
        }

        private UniTask FetchQuiz(string id)
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = new CancellationTokenSource();

            return _fetchQuizUseCase
                .Execute(
                    new FetchQuiz.Request { Id = id },
                    _cancellationTokenSource.Token
                )
                .Tap(_ =>
                {
                    var activeQuiz = _activeQuizRepository.Get();

                    QuizName.Value = activeQuiz.Name;

                    _topics.Clear();
                    _topics.AddRange(activeQuiz.Topics);

                    AdvanceToNextQuestion();
                })
                .CatchAll(error => _appMediator.TechnicalErrorOccured.Invoke(error));
        }

        private async UniTask SubmitAnswer()
        {
            var selectedOption = _options.Items.FirstOrDefault(o => o.IsSelected.Value);

            if (selectedOption is null)
            {
                return;
            }

            var selectedIndex = _options.Items.IndexOf(selectedOption);

            var submittedQuestionId = _activeQuizRepository.Get().GetActiveQuestion().Id;

            await _submitAnswerUseCase
                .Execute(
                    new SubmitAnswer.Request
                    {
                        Id = _activeQuizRepository.Get().Id,
                        QuestionId = submittedQuestionId,
                        SelectedIndex = selectedIndex
                    },
                    _cancellationTokenSource.Token
                )
                .Tap(_ => { AdvanceToNextQuestion(); })
                .CatchAll(error => _appMediator.TechnicalErrorOccured.Invoke(error));
        }

        private void AdvanceToNextQuestion()
        {
            var quiz = _activeQuizRepository.Get();

            if (quiz is null)
            {
                return;
            }

            Progress.Value = quiz.GetProgress();

            var nextQuestion = quiz.GetActiveQuestion();

            if (nextQuestion is null)
            {
                var quizFinishedViewModel = new QuizFinishedDialogViewModel(
                    _goToFinishedQuizCommand,
                    _goBackCommand
                );

                _quizzesMediator.CreateQuizFinishedDialog(quizFinishedViewModel);

                return;
            }

            _questionText.Value = nextQuestion.Text;

            var options = nextQuestion.Options
                .Select((optionText, index) => new QuizOptionViewModel(
                    optionText,
                    ((char)('A' + index)).ToString()
                ))
                .ToList();

            _options.Set(options);
        }

        public void Exit()
        {
            _payload = null;
            _activeQuizRepository.Clear();

            _uiStackMediator.PopAllScreens();
            _uiStackMediator.PopAllDialogs();

            GoBackCommand.Dispose();
            ContinueCommand.Dispose();
            _goToFinishedQuizCommand.Dispose();
            _goBackCommand.Dispose();

            _topics.Dispose();
            _options.Dispose();
            _progress.Dispose();
            _quizName.Dispose();
            _questionText.Dispose();

            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
        }
    }
}