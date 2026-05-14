using Source.Shared.StateMachine.States;

namespace Source.Shared.StateMachine
{
    public interface IStackStateMachine<in T> : IPayloadStateMachine<T> where T : IState
    {
        void Pop();
    }
}