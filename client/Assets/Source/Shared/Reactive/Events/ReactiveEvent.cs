using System;
using System.Collections.Generic;
using System.Linq;
using Source.Shared.Reactive.Disposables;

namespace Source.Shared.Reactive.Events
{
    public sealed class ReactiveEvent<T> : IReactiveEvent<T>, IReadOnlyReactiveEvent<T>
    {
        private Subscription<T>[] _subscribers = Array.Empty<Subscription<T>>();

        public IDisposable Subscribe(Action<T> action)
        {
            var subscriber = new Subscription<T>(action);

            var current = _subscribers;

            var newArray = new Subscription<T>[current.Length + 1];

            Array.Copy(current, newArray, current.Length);

            newArray[current.Length] = subscriber;

            _subscribers = newArray;

            return new Disposable(() => Unsubscribe(subscriber));
        }

        public void Invoke(T value)
        {
            var snapshot = _subscribers;

            foreach (var t in snapshot)
            {
                t.Invoke(value);
            }
        }

        private void Unsubscribe(Subscription<T> subscription)
        {
            var current = _subscribers;
            
            var index = Array.IndexOf(current, subscription);

            if (index < 0)
            {
                return;
            }

            var newArray = new Subscription<T>[current.Length - 1];

            if (index > 0)
            {
                Array.Copy(current, newArray, index);
            }

            if (index < current.Length - 1)
            {
                Array.Copy(current, index + 1, newArray, index, current.Length - index - 1);
            }

            _subscribers = newArray;
        }

        public void Dispose() =>
            _subscribers = Array.Empty<Subscription<T>>();
    }
}