using Source.Shared.StateMachine.States;

namespace Source.App.StateMachine.States.MainState.StateMachine
{
    public abstract class ApplicationState : IState
    {
        protected ApplicationStateMachine StateMachine;
        public void Bind(ApplicationStateMachine stateMachine) =>
            StateMachine = stateMachine;
    }
}