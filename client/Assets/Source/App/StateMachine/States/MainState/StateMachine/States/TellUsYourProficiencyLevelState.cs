using System;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Source.App.Mediator;
using Source.Features.Auth.UseCases;
using Source.Features.Onboarding.Mediator;
using Source.Features.Onboarding.Models;
using Source.Features.Onboarding.TellUsYourProficiency.ViewModels;
using Source.Features.Onboarding.UseCases;
using Source.Shared;
using Source.Shared.Extensions;
using Source.Shared.Reactive.Commands;
using Source.Shared.Reactive.List;
using Source.Shared.Reactive.SelectableList;
using Source.Shared.StateMachine.States;
using Source.Shared.UIStack.Mediator;

namespace Source.App.StateMachine.States.MainState.StateMachine.States
{
    public class TellUsYourProficiencyLevelStatePayload
    {
        public Action GoBackAction { get; set; }
    }

    public class TellUsYourProficiencyLevelState :
        ApplicationState,
        IPayloadState<TellUsYourProficiencyLevelStatePayload>,
        IExitState,
        ITellUsYourProficiencyScreenViewModel
    {
        public IReadOnlyReactiveList<ProficiencyItemViewModel> Proficiencies => _proficiencies.Items;
        public ICommand<ProficiencyItemViewModel> SelectProficiencyCommand => _proficiencies.SelectCommand;
        public ICommand ContinueCommand { get; private set; }
        public ICommand GoBackCommand { get; private set; }
        public int CurrentStep => _onboardingStore.Get().CurrentStep;
        public int TotalSteps => _onboardingStore.Get().TotalSteps;

        private readonly IOnboardingMediator _onboardingMediator;
        private readonly IScreenStackMediator _screenStackMediator;
        private readonly IOnboardingStore _onboardingStore;
        private readonly IAppMediator _appMediator;
        private readonly IUseCase<SubmitOnboarding.Request, SubmitOnboarding.Response> _submitOnboardingUseCase;
        private readonly IUseCase<RefreshAccessToken.Response> _refreshTokenUseCase;

        private readonly SelectableList<ProficiencyItemViewModel> _proficiencies = SelectableList<ProficiencyItemViewModel>.Exclusive();

        private CancellationTokenSource _cancellationTokenSource;
        private TellUsYourProficiencyLevelStatePayload _payload;

        public TellUsYourProficiencyLevelState(
            IOnboardingMediator onboardingMediator,
            IScreenStackMediator screenStackMediator,
            IOnboardingStore onboardingStore,
            IAppMediator appMediator,
            IUseCase<SubmitOnboarding.Request, SubmitOnboarding.Response> submitOnboardingUseCase,
            IUseCase<RefreshAccessToken.Response> refreshTokenUseCase
        )
        {
            _appMediator = appMediator;
            _onboardingMediator = onboardingMediator;
            _screenStackMediator = screenStackMediator;
            _onboardingStore = onboardingStore;
            _submitOnboardingUseCase = submitOnboardingUseCase;
            _refreshTokenUseCase = refreshTokenUseCase;
        }

        public void Enter(TellUsYourProficiencyLevelStatePayload payload)
        {
            _payload = payload;

            _cancellationTokenSource = new CancellationTokenSource();

            ContinueCommand = AsyncCommand.Create(() => SubmitOnboarding());

            GoBackCommand = SyncCommand.Create(() =>
            {
                _onboardingStore.Get().RevertStep();
                _payload.GoBackAction?.Invoke();
            });

            _onboardingMediator
                .CreateTellUsYourProficiencyScreen(this)
                .Forget();

            _proficiencies.Set(_onboardingStore.Get().ProficiencyModels.Select(i => new ProficiencyItemViewModel(i)).ToList());
        }

        private async UniTask<Result> SubmitOnboarding()
        {
            var onboarding = _onboardingStore.Get();

            var result = await _submitOnboardingUseCase
                .Execute(
                    new SubmitOnboarding.Request
                    {
                        Goals = onboarding.SelectedGoals.Select(g => g.Id).ToList(),
                        Interests = onboarding.SelectedInterests.Select(i => i.Id).ToList(),
                        Proficiency = _proficiencies.Items.FirstOrDefault(i => i.IsSelected.Value)?.Id
                    },
                    _cancellationTokenSource.Token
                )
                .AsResult()
                .Recover(Features.Onboarding.UseCases.SubmitOnboarding.ErrorCodes.OnboardingAlreadyCompleted, _ => UniTask.FromResult(Result.Success()))
                .Bind(() =>
                {
                    return _refreshTokenUseCase
                        .Execute(_cancellationTokenSource.Token)
                        .Tap(_ => _onboardingStore.Clear());
                })
                .Tap(_ => StateMachine.Enter<MyQuizzesState>())
                .CatchAll(error => _appMediator.TechnicalErrorOccured.Invoke(error));

            return result;
        }

        public void Exit()
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;

            _proficiencies.Dispose();

            _screenStackMediator.PopAllScreens();

            GoBackCommand.Dispose();
            ContinueCommand.Dispose();
        }
    }
}