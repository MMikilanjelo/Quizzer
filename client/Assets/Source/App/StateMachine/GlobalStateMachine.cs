using System;
using Source.App.StateMachine.States.BootstrapState;
using Source.App.StateMachine.States.MainState;
using Source.Shared.StateMachine;

namespace Source.App.StateMachine
{
    public class GlobalStateMachine : StateMachine<GlobalState>
    {
        public GlobalStateMachine(
            BootstrapState bootstrapState,
            MainState mainState
        )
        {
            bootstrapState.Bind(this);
            mainState.Bind(this);
            
            RegisterState(bootstrapState);
            RegisterState(mainState);
        }
    }
}