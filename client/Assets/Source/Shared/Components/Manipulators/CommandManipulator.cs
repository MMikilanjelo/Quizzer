using System;
using Cysharp.Threading.Tasks;
using Source.Shared.Reactive.Commands;
using UnityEngine.UIElements;

namespace Source.Shared.Components.Manipulators
{
    public sealed class CommandManipulator : Manipulator
    {
        private readonly ICommand _command;
        private readonly Clickable _clickable;
        private IDisposable _commandSubscription;

        public CommandManipulator(ICommand command)
        {
            _command = command;
            _clickable = new Clickable(OnClicked);
        }

        protected override void RegisterCallbacksOnTarget()
        {
            target.AddManipulator(_clickable);

            _commandSubscription = _command.State.Subscribe(OnCommandStateChanged);
            OnCommandStateChanged(_command.State.Value);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            target.RemoveManipulator(_clickable);
            _commandSubscription?.Dispose();
        }

        private void OnCommandStateChanged(CommandState newState)
        {
            if (target == null || _command == null)
            {
                return;
            }

            var isLoading = newState == CommandState.Executing;

            var isDisabled = newState == CommandState.Disabled;

            target.SetEnabled(!isDisabled);

            target.pickingMode = isLoading ? PickingMode.Ignore : PickingMode.Position;

            using var evt = CommandStateChangedEvent.GetPooled(newState);

            evt.target = target;

            target.SendEvent(evt);
        }

        private void OnClicked()
        {
            if (_command?.State.Value == CommandState.Ready)
            {
                _command.Execute().Forget();
            }
        }
    }

    public sealed class CommandManipulator<T> : Manipulator
    {
        private readonly ICommand<T> _command;
        private readonly Clickable _clickable;
        private readonly Func<T> _payloadProvider;
        private IDisposable _commandSubscription;

        public CommandManipulator(ICommand<T> command, Func<T> payloadProvider)
        {
            _command = command;
            _payloadProvider = payloadProvider;
            _clickable = new Clickable(OnClicked);
        }

        protected override void RegisterCallbacksOnTarget()
        {
            target.AddManipulator(_clickable);

            _commandSubscription = _command.State.Subscribe(OnCommandStateChanged);
            OnCommandStateChanged(_command.State.Value);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            target.RemoveManipulator(_clickable);
            _commandSubscription?.Dispose();
        }

        private void OnCommandStateChanged(CommandState newState)
        {
            if (target == null || _command == null)
            {
                return;
            }

            var isLoading = newState == CommandState.Executing;

            var isDisabled = newState == CommandState.Disabled;

            target.SetEnabled(!isDisabled);

            target.pickingMode = isLoading ? PickingMode.Ignore : PickingMode.Position;

            using var evt = CommandStateChangedEvent.GetPooled(newState);

            evt.target = target;

            target.SendEvent(evt);
        }

        private void OnClicked()
        {
            if (_command?.State.Value == CommandState.Ready)
            {
                var payload = _payloadProvider();

                _command.Execute(payload).Forget();
            }
        }
    }

    public sealed class CommandStateChangedEvent : EventBase<CommandStateChangedEvent>
    {
        public CommandState State { get; private set; }

        public static CommandStateChangedEvent GetPooled(CommandState state)
        {
            var evt = GetPooled();
            evt.State = state;
            return evt;
        }

        protected override void Init()
        {
            base.Init();
            LocalInit();
        }

        private void LocalInit() =>
            State = CommandState.Ready;
    }
}