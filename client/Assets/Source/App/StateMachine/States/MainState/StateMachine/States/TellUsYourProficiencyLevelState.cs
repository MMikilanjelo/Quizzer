using System;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Source.Features.Onboarding.Mediator;
using Source.Features.Onboarding.Models;
using Source.Features.Onboarding.TellUsYourProficiency.ViewModels;
using Source.Features.Onboarding.UseCases;
using Source.Features.TechnicalDialogs.Mediator;
using Source.Shared;
using Source.Shared.Extensions;
using Source.Shared.Reactive.Commands;
using Source.Shared.Reactive.List;
using Source.Shared.Reactive.SelectableList;
using Source.Shared.Services;
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
        IPayloadState<TellUsYourInterestsStatePayload>,
        IExitState,
        ITellUsYourProficiencyScreenViewModel
    {
        public IReadOnlyReactiveList<ProficiencyItemViewModel> Proficiencies => _proficiencies.Items;
        public ICommand<ProficiencyItemViewModel> SelectProficiencyCommand => _proficiencies.SelectCommand;
        public ICommand ContinueCommand { get; private set; }
        public ICommand GoBackCommand { get; private set; }
        public int CurrentStep => _onboardingRepository.Get().CurrentStep;
        public int TotalSteps => _onboardingRepository.Get().TotalSteps;

        private readonly IOnboardingMediator _onboardingMediator;
        private readonly IScreenStackMediator _screenStackMediator;
        private readonly IOnboardingRepository _onboardingRepository;
        private readonly IUseCase<SubmitOnboarding.Response> _submitOnboardingUseCase;

        private readonly SelectableList<ProficiencyItemViewModel> _proficiencies = SelectableList<ProficiencyItemViewModel>.Exclusive();

        private CancellationTokenSource _cancellationTokenSource;
        private TellUsYourInterestsStatePayload _payload;

        public TellUsYourProficiencyLevelState(
            IOnboardingMediator onboardingMediator,
            IScreenStackMediator screenStackMediator,
            IOnboardingRepository onboardingRepository,
            IUseCase<SubmitOnboarding.Response> submitOnboardingUseCase
        )
        {
            _onboardingMediator = onboardingMediator;
            _screenStackMediator = screenStackMediator;
            _onboardingRepository = onboardingRepository;
            _submitOnboardingUseCase = submitOnboardingUseCase;
        }

        public void Enter(TellUsYourInterestsStatePayload payload)
        {
            _payload = payload;

            _cancellationTokenSource = new CancellationTokenSource();

            ContinueCommand = AsyncCommand.Create(() => SubmitOnboarding());

            GoBackCommand = SyncCommand.Create(() =>
            {
                _onboardingRepository.Get().RevertStep();
                _payload.GoBackAction?.Invoke();
            });

            _onboardingMediator
                .CreateTellUsYourProficiencyScreen(this)
                .Forget();

            _proficiencies.Set(_onboardingRepository.Get().ProficiencyModels.Select(i => new ProficiencyItemViewModel(i)).ToList());
        }

        private async UniTask<Result> SubmitOnboarding()
        {
            var result = await _submitOnboardingUseCase
                .Execute(_cancellationTokenSource.Token)
                .Tap(_ => StateMachine.Enter<HomeState>());

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