using System.Collections.Generic;
using Source.Shared.Reactive.Events;

namespace Source.Shared.Reactive.Dictionary
{
    public interface IReadOnlyReactiveDictionary<TKey, TValue> : IReadOnlyDictionary<TKey, TValue>
    {
        IReadOnlyReactiveEvent<KeyValuePair<TKey, TValue>> Added { get; }
        IReadOnlyReactiveEvent<KeyValuePair<TKey, TValue>> Removed { get; }
        IReadOnlyReactiveEvent<KeyValuePair<TKey, TValue>> Changed { get; }
        IReadOnlyReactiveEvent<EmptyEvent> Cleared { get; }
    }
}