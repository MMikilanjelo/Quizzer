using System;

namespace Source.Shared.Reactive.Events
{
    public interface IReadOnlyReactiveProperty<out T>
    {
        T Value { get; }
        IDisposable Subscribe(Action<T> action);
    }
}