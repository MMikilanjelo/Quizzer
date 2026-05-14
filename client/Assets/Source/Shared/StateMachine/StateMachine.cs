using System;
using System.Collections.Generic;
using Source.Shared.StateMachine.States;

namespace Source.Shared.StateMachine
{
    public abstract class StateMachine<T> : IPayloadStateMachine<T> where T : IState
    {
        private readonly Dictionary<Type, T> _states = new();
        
        private T _state;

        public void RegisterState(T state)
        {
            if (state == null)
                return;

            var type = state.GetType();

            _states.TryAdd(type, state);
        }

        public void Enter<TState>() where TState : class, T, IEnterState
        {
            if (!TryChangeState<TState>())
                return;

            if (_state is IEnterState enterState)
                enterState.Enter();
        }

        public void Enter<TState, TPayload>(TPayload payload)
            where TState : class, T, IPayloadState<TPayload>
        {
            if (!TryChangeState<TState>())
                return;

            if (_state is IPayloadState<TPayload> payloadState)
                payloadState.Enter(payload);
        }

        public void ExitStateMachine()
        {
            if (_state is IExitState exitState)
                exitState.Exit();

            _state = default;
        }

        private bool TryChangeState<TState>() where TState : class, T
        {
            var type = typeof(TState);

            if (!_states.TryGetValue(type, out var next))
                return false;

            if (_state is IExitState exitState)
                exitState.Exit();

            _state = next;
            return true;
        }
    }
}