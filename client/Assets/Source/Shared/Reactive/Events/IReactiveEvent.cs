using System;

namespace Source.Shared.Reactive.Events
{
    public interface IReactiveEvent<T> : IDisposable
    {
        void Invoke(T value);
        IDisposable Subscribe(Action<T> action);
    }
}