using System.Collections.Generic;
using Source.Shared.Reactive.Events;

namespace Source.Shared.Reactive.List
{
    public interface IReadOnlyReactiveList<T> : IReadOnlyList<T>
    {
        IReadOnlyReactiveEvent<T> Added { get; }
        IReadOnlyReactiveEvent<T> Removed { get; }
        IReadOnlyReactiveEvent<ValueChangedEvent<T>> Replaced { get; }
        IReadOnlyReactiveEvent<EmptyEvent> Cleared { get; }
    }
}