using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Source.App.Mediator;
using Source.Features.MyProfile.Mediator;
using Source.Features.MyProfile.UseCase;
using Source.Features.MyProfile.ViewModels;
using Source.Shared;
using Source.Shared.Extensions;
using Source.Shared.Reactive.Events;
using Source.Shared.Reactive.List;
using Source.Shared.StateMachine.States;
using Source.Shared.UIStack.Mediator;
using UnityEngine;

namespace Source.App.StateMachine.States.MainState.StateMachine.States
{
    public class MyProfileState :
        ApplicationState,
        IEnterState,
        IExitState,
        IMyProfileScreenViewModel
    {
        public IReadOnlyReactiveProperty<bool> IsLoading => _isLoading;
        public IReadOnlyReactiveProperty<float> AverageScore => _averageScore;
        public IReadOnlyReactiveProperty<int> TotalQuizzes => _totalQuizzes;
        public IReadOnlyReactiveProperty<int> PerfectQuizzes => _perfectQuizzes;
        public IReadOnlyReactiveProperty<int> StreakDays => _streakDays;
        public IReadOnlyReactiveList<ConceptMasteryLevelViewModel> TopStrengths => _topStrengths;
        public IReadOnlyReactiveList<ConceptMasteryLevelViewModel> FocusAreas => _focusAreas;

        private readonly ReactiveProperty<bool> _isLoading = new(true);
        private readonly ReactiveProperty<float> _averageScore = new(0f);
        private readonly ReactiveProperty<int> _totalQuizzes = new(0);
        private readonly ReactiveProperty<int> _perfectQuizzes = new(0);
        private readonly ReactiveProperty<int> _streakDays = new(0);
        private readonly ReactiveList<ConceptMasteryLevelViewModel> _topStrengths = new();
        private readonly ReactiveList<ConceptMasteryLevelViewModel> _focusAreas = new();

        private readonly IMyProfileMediator _myProfileMediator;
        private readonly IAppMediator _appMediator;
        private readonly IUIStackMediator _uiStackMediator;
        private readonly IUseCase<FetchMyProfile.Request, FetchMyProfile.Response> _fetchMyProfileUseCase;

        private CancellationTokenSource _cancellationTokenSource;

        public MyProfileState(
            IMyProfileMediator myProfileMediator,
            IAppMediator appMediator,
            IUIStackMediator uiStackMediator,
            IUseCase<FetchMyProfile.Request, FetchMyProfile.Response> fetchMyProfileUseCase
        )
        {
            _myProfileMediator = myProfileMediator;
            _appMediator = appMediator;
            _uiStackMediator = uiStackMediator;
            _fetchMyProfileUseCase = fetchMyProfileUseCase;
        }

        public void Enter()
        {
            _myProfileMediator.CreateMyProfileScreen(this).Forget();

            FetchProfileDataAsync().Forget();
        }

        private UniTask FetchProfileDataAsync()
        {
            _isLoading.Value = true;

            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = new CancellationTokenSource();

            return _fetchMyProfileUseCase
                .Execute(new FetchMyProfile.Request(), _cancellationTokenSource.Token)
                .Tap(response => BindProfileData(response.Dashboard))
                .CatchAll(error => _appMediator.TechnicalErrorOccured.Invoke(error))
                .Finally(() => _isLoading.Value = false);
        }

        private void BindProfileData(FetchMyProfile.UserDashboardModel data)
        {
            _averageScore.Value = data.AverageScore;
            _totalQuizzes.Value = data.TotalQuizzes;
            _perfectQuizzes.Value = data.PerfectQuizzes;
            _streakDays.Value = data.StreakDays;

            _topStrengths.Clear();
            var mappedStrengths = data.TopStrengths.Select(m => new ConceptMasteryLevelViewModel
            {
                ConceptId = m.TopicId,
                MasteryPercentage = m.MasteryPercentage
            });
            _topStrengths.AddRange(mappedStrengths);

            _focusAreas.Clear();
            var mappedFocusAreas = data.FocusAreas.Select(m => new ConceptMasteryLevelViewModel
            {
                ConceptId = m.TopicId,
                MasteryPercentage = m.MasteryPercentage
            });
            _focusAreas.AddRange(mappedFocusAreas);
        }

        public void Exit()
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;

            _uiStackMediator.PopAllScreens();
            _uiStackMediator.PopAllDialogs();

            _isLoading.Dispose();
            _averageScore.Dispose();
            _totalQuizzes.Dispose();
            _perfectQuizzes.Dispose();
            _streakDays.Dispose();

            _topStrengths.Dispose();
            _focusAreas.Dispose();
        }
    }
}