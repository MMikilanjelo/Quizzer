using System;

namespace Source.Shared.Reactive.Events
{
    public sealed class Subscription<T>
    {
        private readonly Action<T> _action;

        public Subscription(Action<T> action) =>
            _action = action;

        public void Invoke(T value) =>
            _action?.Invoke(value);
    }
}