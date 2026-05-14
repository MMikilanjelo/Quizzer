using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Source.Shared.Reactive.Events;

namespace Source.Shared.Reactive.List
{
    public class ReactiveList<T> : IList<T>, IReadOnlyReactiveList<T>, IDisposable
    {
        public IReadOnlyReactiveEvent<T> Added => _added;
        public IReadOnlyReactiveEvent<T> Removed => _removed;
        public IReadOnlyReactiveEvent<ValueChangedEvent<T>> Replaced => _replaced;
        public IReadOnlyReactiveEvent<EmptyEvent> Cleared => _cleared;

        private readonly ReactiveEvent<T> _added = new();
        private readonly ReactiveEvent<T> _removed = new();
        private readonly ReactiveEvent<ValueChangedEvent<T>> _replaced = new();
        private readonly ReactiveEvent<EmptyEvent> _cleared = new();

        private readonly List<T> _source;

        public ReactiveList() => _source = new List<T>();
        public ReactiveList(int capacity) => _source = new List<T>(capacity);
        public ReactiveList(IEnumerable<T> collection) => _source = new List<T>(collection);

        public T this[int index]
        {
            get => _source[index];
            set
            {
                T old = _source[index];
                if (EqualityComparer<T>.Default.Equals(old, value)) return;

                _source[index] = value;
                _replaced.Invoke(new ValueChangedEvent<T>(old, value));
            }
        }

        public int Count => _source.Count;

        public bool IsReadOnly => false;

        public void Add(T item)
        {
            _source.Add(item);
            _added.Invoke(item);
        }

        public void AddRange(IEnumerable<T> collection)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            var itemsToAdd = collection as List<T> ?? collection.ToList();

            if (_source.Capacity < _source.Count + itemsToAdd.Count)
            {
                _source.Capacity = _source.Count + itemsToAdd.Count;
            }

            foreach (var item in itemsToAdd)
            {
                _source.Add(item);
                _added.Invoke(item);
            }
        }

        public bool Remove(T item)
        {
            if (_source.Remove(item))
            {
                _removed.Invoke(item);
                return true;
            }

            return false;
        }

        public int RemoveAll(Func<T, bool> match)
        {
            int removedCount = 0;

            for (int i = _source.Count - 1; i >= 0; i--)
            {
                T item = _source[i];

                if (match(item))
                {
                    _source.RemoveAt(i);

                    _removed.Invoke(item);

                    removedCount++;
                }
            }

            return removedCount;
        }

        public void RemoveAt(int index)
        {
            if (index < 0 || index >= _source.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            T item = _source[index];

            _source.RemoveAt(index);

            _removed.Invoke(item);
        }

        public void Insert(int index, T item)
        {
            _source.Insert(index, item);
            _added.Invoke(item);
        }

        public void Clear()
        {
            var itemsToRemove = _source.ToList();

            _source.Clear();

            foreach (var item in itemsToRemove)
            {
                _removed.Invoke(item);
            }

            _cleared.Invoke(new EmptyEvent());
        }

        public bool Contains(T item) => _source.Contains(item);
        public int IndexOf(T item) => _source.IndexOf(item);
        public void CopyTo(T[] array, int arrayIndex) => _source.CopyTo(array, arrayIndex);
        public IEnumerator<T> GetEnumerator() => _source.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => _source.GetEnumerator();

        public void Dispose()
        {
            _added.Dispose();
            _removed.Dispose();
            _replaced.Dispose();
            _cleared.Dispose();
            _source.Clear();
        }
    }
}