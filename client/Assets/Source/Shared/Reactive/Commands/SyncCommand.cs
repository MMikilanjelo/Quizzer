using System;
using Cysharp.Threading.Tasks;
using Source.Shared.Reactive.Events;

namespace Source.Shared.Reactive.Commands
{
    public sealed class SyncCommand : ICommand, IDisposable
    {
        public IReadOnlyReactiveEvent<EmptyEvent> Executed => _executed;
        public IReadOnlyReactiveProperty<CommandState> State => _state;

        private readonly ReactiveProperty<CommandState> _state = new(CommandState.Ready);
        private readonly ReactiveEvent<EmptyEvent> _executed = new();

        private readonly Action _execute;

        private IReadOnlyReactiveProperty<bool> _executionRule;

        private IDisposable _ruleDisposable;

        private SyncCommand(Action execute) => _execute = execute;
        public static SyncCommand Create(Action execute) => new(execute);

        public SyncCommand WithExecutionRule(IReadOnlyReactiveProperty<bool> ruleProperty)
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

        public UniTask Execute()
        {
            if (_state.Value != CommandState.Ready) return UniTask.CompletedTask;

            _state.Value = CommandState.Executing;

            try
            {
                _execute();
            }
            finally
            {
                var canExecute = _executionRule == null || _executionRule.Value;

                _state.Value = canExecute ? CommandState.Ready : CommandState.Disabled;

                _executed.Invoke(EmptyEvent.Default);
            }

            return UniTask.CompletedTask;
        }

        public void Dispose()
        {
            _ruleDisposable?.Dispose();
            _executed.Dispose();
        }
    }

    public sealed class SyncCommand<T> : ICommand<T>, IDisposable
    {
        public IReadOnlyReactiveEvent<EmptyEvent> Executed => _executed;
        public IReadOnlyReactiveProperty<CommandState> State => _state;

        private readonly ReactiveProperty<CommandState> _state = new(CommandState.Ready);
        private readonly ReactiveEvent<EmptyEvent> _executed = new();

        private readonly Action<T> _execute;

        private IReadOnlyReactiveProperty<bool> _executionRule;

        private IDisposable _ruleDisposable;

        private SyncCommand(Action<T> execute) => _execute = execute;

        public static SyncCommand<T> Create(Action<T> execute) => new(execute);

        public SyncCommand<T> WithExecutionRule(IReadOnlyReactiveProperty<bool> ruleProperty)
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

        public UniTask Execute(T payload)
        {
            if (_state.Value != CommandState.Ready) return UniTask.CompletedTask;

            _state.Value = CommandState.Executing;

            try
            {
                _execute(payload);
            }
            finally
            {
                var canExecute = _executionRule == null || _executionRule.Value;

                _state.Value = canExecute ? CommandState.Ready : CommandState.Disabled;

                _executed.Invoke(EmptyEvent.Default);
            }

            return UniTask.CompletedTask;
        }

        public void Dispose()
        {
            _ruleDisposable?.Dispose();
            _executed.Dispose();
        }
    }
}