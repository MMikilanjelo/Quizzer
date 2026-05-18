using System.Threading;
using Cysharp.Threading.Tasks;
using Source.App.Mediator;
using Source.Features.Auth.SigIn.Mediator;
using Source.Features.Auth.SigIn.UseCases;
using Source.Features.Auth.SigIn.ViewModels;
using Source.Features.Onboarding.TellUsYourGoal.ViewModels;
using Source.Features.Onboarding.UseCases;
using Source.Features.TechnicalDialogs.ErrorDialog;
using Source.Features.TechnicalDialogs.Mediator;
using Source.Shared;
using Source.Shared.Extensions;
using Source.Shared.Reactive.Commands;
using Source.Shared.Services;
using Source.Shared.StateMachine.States;
using Source.Shared.UIStack.Mediator;

namespace Source.App.StateMachine.States.MainState.StateMachine.States
{
    public class SignInState :
        ApplicationState,
        IEnterState,
        IExitState,
        ISignInScreenViewModel
    {
        public ICommand SignUpCommand { get; private set; }

        private readonly ISignInMediator _mediator;
        private readonly IUIStackMediator _uiStackMediator;
        private readonly IAppMediator _appMediator;

        private readonly IUseCase<CreateGuestAccount.Response> _registerGuestAccountUseCase;
        private readonly IUseCase<LoginGuestAccount.Response> _loginGuestAccountUseCase;
        private readonly IUseCase<LoadOnboardingQuestionnaire.Response> _fetchGoalsUseCase;

        private CancellationTokenSource _cancellationTokenSource;
        private static string Scope => nameof(SignInState);

        public SignInState(
            ISignInMediator signInMediator,
            IUIStackMediator uiStackMediator,
            IUseCase<CreateGuestAccount.Response> registerGuestAccountUseCase,
            IUseCase<LoginGuestAccount.Response> loginGuestAccountUseCase,
            IUseCase<LoadOnboardingQuestionnaire.Response> fetchGoalsUseCase,
            IAppMediator appMediator
        )
        {
            _mediator = signInMediator;
            _uiStackMediator = uiStackMediator;
            _registerGuestAccountUseCase = registerGuestAccountUseCase;
            _loginGuestAccountUseCase = loginGuestAccountUseCase;
            _fetchGoalsUseCase = fetchGoalsUseCase;
            _appMediator = appMediator;
        }

        public void Enter()
        {
            _cancellationTokenSource = new CancellationTokenSource();

            SignUpCommand = AsyncCommand.Create(() => SignUp());

            SignIn().Forget();
        }

        public void Exit()
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;

            _uiStackMediator.PopAll();

            SignUpCommand.Dispose();
        }

        private async UniTask<Result> SignIn()
        {
            return await _loginGuestAccountUseCase
                .Execute(_cancellationTokenSource.Token)
                .Bind(async response =>
                {
                    if (response.IsOnboardingCompletionRequired)
                    {
                        return await _fetchGoalsUseCase
                            .Execute(_cancellationTokenSource.Token)
                            .Tap(_ => StateMachine.Enter<TellUsYourGoalState>());
                    }

                    StateMachine.Enter<MyQuizzesState>();

                    return Result.Success();
                })
                .Recover(LoginGuestAccount.ErrorCodes.UserNotRegistered, _ => _mediator.CreateSignInScreen(Scope, this))
                .CatchAll(error => _appMediator.TechnicalErrorOccured.Invoke(error));
        }

        private async UniTask<Result> SignUp()
        {
            return await _registerGuestAccountUseCase
                .Execute(_cancellationTokenSource.Token)
                .Catch(CreateGuestAccount.ErrorCodes.AlreadyRegister, _ => SignIn())
                .Bind(_ => SignIn())
                .CatchAll(error => _appMediator.TechnicalErrorOccured.Invoke(error));
        }
    }
}