using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Source.Shared.Reactive.Events;

namespace Source.Shared.Reactive.Dictionary
{
    public class ReactiveDictionary<TKey, TValue> : IDictionary<TKey, TValue>, IReadOnlyReactiveDictionary<TKey, TValue>, IDisposable
    {
        public IReadOnlyReactiveEvent<KeyValuePair<TKey, TValue>> Added => _added;
        public IReadOnlyReactiveEvent<KeyValuePair<TKey, TValue>> Removed => _removed;
        public IReadOnlyReactiveEvent<KeyValuePair<TKey, TValue>> Changed => _changed;
        public IReadOnlyReactiveEvent<EmptyEvent> Cleared => _cleared;

        private readonly ReactiveEvent<KeyValuePair<TKey, TValue>> _added = new();
        private readonly ReactiveEvent<KeyValuePair<TKey, TValue>> _removed = new();
        private readonly ReactiveEvent<KeyValuePair<TKey, TValue>> _changed = new();
        private readonly ReactiveEvent<EmptyEvent> _cleared = new();

        private readonly Dictionary<TKey, TValue> _source;

        public ReactiveDictionary() => _source = new Dictionary<TKey, TValue>();
        public ReactiveDictionary(IEqualityComparer<TKey> comparer) => _source = new Dictionary<TKey, TValue>(comparer);
        public ReactiveDictionary(IDictionary<TKey, TValue> dictionary) => _source = new Dictionary<TKey, TValue>(dictionary);

        public int Count => _source.Count;
        public bool IsReadOnly => false;
        public ICollection<TKey> Keys => _source.Keys;
        public ICollection<TValue> Values => _source.Values;
        IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => _source.Keys;
        IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => _source.Values;

        public TValue this[TKey key]
        {
            get => _source[key];
            set
            {
                if (_source.TryGetValue(key, out var oldValue))
                {
                    if (!EqualityComparer<TValue>.Default.Equals(oldValue, value))
                    {
                        _source[key] = value;
                        _changed.Invoke(new KeyValuePair<TKey, TValue>(key, value));
                    }
                }
                else
                {
                    Add(key, value);
                }
            }
        }

        public void Add(TKey key, TValue value)
        {
            _source.Add(key, value);
            _added.Invoke(new KeyValuePair<TKey, TValue>(key, value));
        }

        public void Add(KeyValuePair<TKey, TValue> item) => Add(item.Key, item.Value);

        public bool Remove(TKey key)
        {
            if (_source.Remove(key, out var value))
            {
                _removed.Invoke(new KeyValuePair<TKey, TValue>(key, value));
                return true;
            }

            return false;
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            if (_source.TryGetValue(item.Key, out var value) && EqualityComparer<TValue>.Default.Equals(value, item.Value))
            {
                Remove(item.Key);
                return true;
            }

            return false;
        }

        public void Clear()
        {
            if (_source.Count == 0) return;

            foreach (var kvp in _source)
            {
                _removed.Invoke(kvp);
            }

            _source.Clear();
            _cleared.Invoke(new EmptyEvent());
        }

        public bool ContainsKey(TKey key) => _source.ContainsKey(key);
        public bool Contains(KeyValuePair<TKey, TValue> item) => ((ICollection<KeyValuePair<TKey, TValue>>)_source).Contains(item);
        public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value) => _source.TryGetValue(key, out value);
        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) => ((ICollection<KeyValuePair<TKey, TValue>>)_source).CopyTo(array, arrayIndex);
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => _source.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => _source.GetEnumerator();

        public void Dispose()
        {
            _added.Dispose();
            _removed.Dispose();
            _changed.Dispose();
            _cleared.Dispose();
            _source.Clear();
        }
    }
}