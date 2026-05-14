using Source.App.StateMachine;
using Source.App.StateMachine.States.BootstrapState;
using Source.Shared.StateMachine;
using VContainer.Unity;

namespace Source.App.Bootstrap
{
    public class Bootstrapper : IStartable
    {
        private readonly IStateMachine<GlobalState> _globalStateMachine;

        public Bootstrapper(GlobalStateMachine globalStateMachine) =>
            _globalStateMachine = globalStateMachine;

        public void Start() =>
            _globalStateMachine.Enter<BootstrapState>();
    }
}