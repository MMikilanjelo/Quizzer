using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Source.App.Mediator;
using Source.Features.Onboarding.Mediator;
using Source.Features.Onboarding.Models;
using Source.Features.Onboarding.TellUsYourGoal.ViewModels;
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
    public class TellUsYourGoalState :
        ApplicationState,
        IEnterState,
        IExitState,
        ITellUsYourGoalScreenViewModel
    {
        public ICommand ContinueCommand { get; private set; }
        public ICommand<YourGoalItemViewModel> SelectGoalCommand => _goals.SelectCommand;
        public IReadOnlyReactiveList<YourGoalItemViewModel> Goals => _goals.Items;
        public int CurrentStep => _onboardingRepository.Get().CurrentStep;
        public int TotalSteps => _onboardingRepository.Get().TotalSteps;

        private readonly IOnboardingMediator _onboardingMediator;
        private readonly IUIStackMediator _uiStackMediator;
        private readonly IOnboardingRepository _onboardingRepository;
        private readonly IAppMediator _appMediator;

        private readonly SelectableList<YourGoalItemViewModel> _goals = SelectableList<YourGoalItemViewModel>.Exclusive();

        private CancellationTokenSource _cancellationTokenSource;

        public TellUsYourGoalState(
            IOnboardingMediator onboardingMediator,
            IUIStackMediator uiStackMediator,
            IOnboardingRepository onboardingRepository,
            IAppMediator appMediator
        )
        {
            _onboardingMediator = onboardingMediator;
            _uiStackMediator = uiStackMediator;
            _onboardingRepository = onboardingRepository;
            _appMediator = appMediator;
        }

        public void Enter()
        {
            _cancellationTokenSource = new CancellationTokenSource();

            ContinueCommand = SyncCommand
                .Create(() =>
                {
                    var selectedGoals = _goals.Items
                        .Where(vm => vm.IsSelected.Value)
                        .Select(vm => vm.Model)
                        .ToList();

                    _onboardingRepository.SaveSelectedGoals(selectedGoals);

                    _onboardingRepository.Get().AdvanceStep();

                    StateMachine.Enter<TellUsYourInterestsState, TellUsYourInterestsStatePayload>(new TellUsYourInterestsStatePayload()
                    {
                        GoBackAction = () => StateMachine.Enter<TellUsYourGoalState>()
                    });
                })
                .WithExecutionRule(_goals.IsValid);

            _onboardingMediator
                .CreateTellUsYourGoalScreen(this)
                .Forget();

            SetGoals();
        }

        public void Exit()
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;

            _goals.Dispose();
            _uiStackMediator.PopAllScreens();

            ContinueCommand.Dispose();
        }

        private void SetGoals()
        {
            var alreadyFetchedModel = _onboardingRepository.Get();

            var selectedGoalIds = new HashSet<string>(alreadyFetchedModel.SelectedGoals.Select(g => g.Id));

            var viewModels = alreadyFetchedModel.GoalModels
                .Select(m =>
                {
                    var isAlreadySelected = selectedGoalIds.Contains(m.Id);

                    return new YourGoalItemViewModel(m, isAlreadySelected);
                })
                .ToList();

            _goals.Set(viewModels);
        }
    }
}