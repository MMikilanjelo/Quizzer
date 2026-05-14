using System.Collections.Generic;
using Source.Shared.Reactive.Events;

namespace Source.Shared.Reactive.HashSet
{
    public interface IReadOnlyReactiveHashSet<out T> : IReadOnlyCollection<T>
    {
        IReadOnlyReactiveEvent<T> Added { get; }
        IReadOnlyReactiveEvent<T> Removed { get; }
        IReadOnlyReactiveEvent<EmptyEvent> Cleared { get; }
    }
}