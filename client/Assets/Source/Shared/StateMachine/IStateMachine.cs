using Source.Shared.StateMachine.States;

namespace Source.Shared.StateMachine
{
    public interface IStateMachine<in T> where T : IState
    {
        void RegisterState(T state);
        void Enter<TState>() where TState : class, T, IEnterState;
        void ExitStateMachine();
    }
}