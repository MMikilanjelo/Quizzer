using System;
using Cysharp.Threading.Tasks;
using Source.Shared.Reactive.Events;

namespace Source.Shared.Reactive.Commands
{
    public sealed class AsyncCommand : ICommand, IDisposable
    {
        public IReadOnlyReactiveEvent<EmptyEvent> Executed => _executed;
        public IReadOnlyReactiveProperty<CommandState> State => _state;

        private readonly ReactiveProperty<CommandState> _state = new(CommandState.Ready);
        private readonly ReactiveEvent<EmptyEvent> _executed = new();

        private readonly Func<UniTask> _execute;

        private IReadOnlyReactiveProperty<bool> _executionRule;

        private IDisposable _ruleDisposable;

        private AsyncCommand(Func<UniTask> execute) => _execute = execute;

        public static AsyncCommand Create(Func<UniTask> execute) => new(execute);

        public AsyncCommand WithExecutionRule(IReadOnlyReactiveProperty<bool> ruleProperty)
        {
            _ruleDisposable?.Dispose();

            _executionRule = ruleProperty;

            _ruleDisposable = _executionRule.Subscribe(canExecute =>
            {
                if (_state.Value != CommandState.Executing)
                {
                    _state.Value = canExecute ? CommandState.Ready : CommandState.Disabled;
                }
            });

            _state.Value = _executionRule.Value ? CommandState.Ready : CommandState.Disabled;

            return this;
        }

        public async UniTask Execute()
        {
            if (_state.Value != CommandState.Ready) return;

            _state.Value = CommandState.Executing;

            try
            {
                await _execute();
            }
            finally
            {
                var canExecute = _executionRule == null || _executionRule.Value;

                _state.Value = canExecute ? CommandState.Ready : CommandState.Disabled;

                _executed.Invoke(EmptyEvent.Default);
            }
        }

        public void Dispose()
        {
            _ruleDisposable?.Dispose();
            _executed.Dispose();
        }
    }

    public sealed class AsyncCommand<T> : ICommand<T>, IDisposable
    {
        public IReadOnlyReactiveEvent<EmptyEvent> Executed => _executed;
        public IReadOnlyReactiveProperty<CommandState> State => _state;

        private readonly ReactiveProperty<CommandState> _state = new(CommandState.Ready);
        private readonly ReactiveEvent<EmptyEvent> _executed = new();

        private readonly Func<T, UniTask> _execute;

        private IReadOnlyReactiveProperty<bool> _executionRule;
        private IDisposable _ruleDisposable;

        private AsyncCommand(Func<T, UniTask> execute) => _execute = execute;

        public static AsyncCommand<T> Create(Func<T, UniTask> execute) => new(execute);

        public AsyncCommand<T> WithExecutionRule(IReadOnlyReactiveProperty<bool> ruleProperty)
        {
            _ruleDisposable?.Dispose();
            _executionRule = ruleProperty;

            _ruleDisposable = _executionRule.Subscribe(canExecute =>
            {
                if (_state.Value != CommandState.Executing)
                {
                    _state.Value = canExecute ? CommandState.Ready : CommandState.Disabled;
                }
            });

            _state.Value = _executionRule.Value ? CommandState.Ready : CommandState.Disabled;

            return this;
        }

        public async UniTask Execute(T payload)
        {
            if (_state.Value != CommandState.Ready) return;

            _state.Value = CommandState.Executing;

            try
            {
                await _execute(payload);
            }
            finally
            {
                bool canExecute = _executionRule == null || _executionRule.Value;
                _state.Value = canExecute ? CommandState.Ready : CommandState.Disabled;
                _executed.Invoke(EmptyEvent.Default);
            }
        }

        public void Dispose()
        {
            _ruleDisposable?.Dispose();
            _executed.Dispose();
        }
    }
}