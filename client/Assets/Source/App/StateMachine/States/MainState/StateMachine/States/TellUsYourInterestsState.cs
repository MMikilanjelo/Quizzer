using System;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Source.App.Mediator;
using Source.Features.Onboarding.Mediator;
using Source.Features.Onboarding.Models;
using Source.Features.Onboarding.TellUsYourInterests.ViewModels;
using Source.Shared.Reactive.Commands;
using Source.Shared.Reactive.List;
using Source.Shared.Reactive.SelectableList;
using Source.Shared.StateMachine.States;
using Source.Shared.UIStack.Mediator;

namespace Source.App.StateMachine.States.MainState.StateMachine.States
{
    public class TellUsYourInterestsStatePayload
    {
        public Action GoBackAction { get; set; }
    }

    public class TellUsYourInterestsState :
        ApplicationState,
        IPayloadState<TellUsYourInterestsStatePayload>,
        IExitState,
        ITellUsYourInterestsScreenViewModel
    {
        public IReadOnlyReactiveList<YourInterestItemViewModel> Interests => _interests.Items;
        public ICommand<YourInterestItemViewModel> SelectInterestCommand => _interests.SelectCommand;
        public ICommand ContinueCommand { get; private set; }
        public ICommand GoBackCommand { get; private set; }
        public int CurrentStep => _onboardingStore.Get().CurrentStep;
        public int TotalSteps => _onboardingStore.Get().TotalSteps;

        private readonly IOnboardingMediator _onboardingMediator;
        private readonly IScreenStackMediator _screenStackMediator;
        private readonly IOnboardingStore _onboardingStore;

        private readonly SelectableList<YourInterestItemViewModel> _interests = SelectableList<YourInterestItemViewModel>.Capped(1, 3);

        private CancellationTokenSource _cancellationTokenSource;
        private TellUsYourInterestsStatePayload _payload;

        public TellUsYourInterestsState(
            IOnboardingMediator onboardingMediator,
            IScreenStackMediator screenStackMediator,
            IOnboardingStore onboardingStore
        )
        {
            _onboardingMediator = onboardingMediator;
            _screenStackMediator = screenStackMediator;
            _onboardingStore = onboardingStore;
        }

        public void Enter(TellUsYourInterestsStatePayload payload)
        {
            _payload = payload;
            _cancellationTokenSource = new CancellationTokenSource();

            ContinueCommand = SyncCommand
                .Create(() =>
                {
                    var onboardingModel = _onboardingStore.Get();

                    var selectedInterest = _interests.Items
                        .Where(vm => vm.IsSelected.Value)
                        .Select(vm => vm.Model)
                        .ToList();

                    onboardingModel.AdvanceStep();

                    onboardingModel.SelectedInterests = selectedInterest;

                    var payloadToPassBack = new TellUsYourInterestsStatePayload
                    {
                        GoBackAction = _payload.GoBackAction
                    };

                    StateMachine.Enter<TellUsYourProficiencyLevelState, TellUsYourProficiencyLevelStatePayload>(new TellUsYourProficiencyLevelStatePayload
                    {
                        GoBackAction = () => StateMachine.Enter<TellUsYourInterestsState, TellUsYourInterestsStatePayload>(payloadToPassBack)
                    });
                })
                .WithExecutionRule(_interests.IsValid);

            GoBackCommand = SyncCommand.Create(() =>
            {
                _onboardingStore.Get().RevertStep();
                _payload.GoBackAction?.Invoke();
            });

            _onboardingMediator
                .CreateTellUsYourInterestsScreen(this)
                .Forget();

            _interests.Set(_onboardingStore.Get().InterestModels.Select(i => new YourInterestItemViewModel(i)).ToList());
        }

        public void Exit()
        {
            _payload = null;

            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;

            _interests.Dispose();
            _screenStackMediator.PopAllScreens();

            GoBackCommand.Dispose();
            ContinueCommand.Dispose();
        }
    }
}