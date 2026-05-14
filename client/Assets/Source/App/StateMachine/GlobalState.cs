using Source.Shared.StateMachine.States;

namespace Source.App.StateMachine
{
    public abstract class GlobalState : IState
    {
        protected GlobalStateMachine StateMachine;
        public void Bind(GlobalStateMachine stateMachine) =>
            StateMachine = stateMachine;
    }
}