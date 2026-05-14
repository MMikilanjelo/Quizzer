using Source.Shared.StateMachine.States;

namespace Source.Shared.StateMachine
{
    public interface IPayloadStateMachine<in T> : IStateMachine<T> where T : IState
    {
        void Enter<TState, TPayload>(TPayload payload) where TState : class, T, IPayloadState<TPayload>;
    }
}