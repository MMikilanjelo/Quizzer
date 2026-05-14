namespace Source.Shared.StateMachine.States
{
    public interface IPayloadState<in T> : IState
    {
        public void Enter(T payload);
    }
}