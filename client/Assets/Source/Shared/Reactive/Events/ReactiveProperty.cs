using System;
using System.Collections.Generic;

namespace Source.Shared.Reactive.Events
{
    public sealed class ReactiveProperty<T> : IReactiveProperty<T>
    {
        public ReactiveProperty(T initialValue, IEqualityComparer<T> comparer = null)
        {
            _value = initialValue;

            _comparer = comparer ?? EqualityComparer<T>.Default;
        }

        public T Value
        {
            get => _value;
            set
            {
                if (_comparer.Equals(_value, value))
                {
                    return;
                }

                _value = value;

                _event.Invoke(value);
            }
        }

        private T _value;

        private readonly IEqualityComparer<T> _comparer;

        private readonly ReactiveEvent<T> _event = new();

        public ReactiveProperty() : this(default, null)
        {
        }

        public ReactiveProperty(IEqualityComparer<T> comparer, T initialValue = default) : this(initialValue, comparer)
        {
        }

        public IDisposable Subscribe(Action<T> action) =>
            _event.Subscribe(action);

        public override string ToString() =>
            _value.ToString();

        public void Dispose() =>
            _event.Dispose();
    }
}