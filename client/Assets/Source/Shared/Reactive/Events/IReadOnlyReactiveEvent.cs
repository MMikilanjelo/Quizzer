using System;

namespace Source.Shared.Reactive.Events
{
    public interface IReadOnlyReactiveEvent<out T>
    {
        IDisposable Subscribe(Action<T> action);
    }
}