using System;
using System.Collections;
using System.Collections.Generic;
using Source.Shared.Reactive.Events;

namespace Source.Shared.Reactive.HashSet
{
    public class ReactiveHashSet<T> : ISet<T>, IReadOnlyReactiveHashSet<T>, IDisposable
    {
        public IReadOnlyReactiveEvent<T> Added => _added;
        public IReadOnlyReactiveEvent<T> Removed => _removed;
        public IReadOnlyReactiveEvent<EmptyEvent> Cleared => _cleared;

        private readonly ReactiveEvent<T> _added = new();
        private readonly ReactiveEvent<T> _removed = new();
        private readonly ReactiveEvent<EmptyEvent> _cleared = new();
        private readonly HashSet<T> _source;

        public ReactiveHashSet() => _source = new HashSet<T>();

        public ReactiveHashSet(IEqualityComparer<T> comparer) => _source = new HashSet<T>(comparer);

        public int Count => _source.Count;

        public bool IsReadOnly => false;

        public bool Add(T item)
        {
            if (_source.Add(item))
            {
                _added.Invoke(item);
                return true;
            }

            return false;
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

        void ICollection<T>.Add(T item)
        {
            if (item == null)
            {
                return;
            }

            Add(item);
        }

        public void Clear()
        {
            foreach (var item in _source) _removed.Invoke(item);
            _source.Clear();
            _cleared.Invoke(new EmptyEvent());
        }

        public bool Contains(T item) => _source.Contains(item);

        public void UnionWith(IEnumerable<T> other)
        {
            foreach (var x in other) Add(x);
        }

        public void IntersectWith(IEnumerable<T> other) => _source.IntersectWith(other);

        public void ExceptWith(IEnumerable<T> other)
        {
            foreach (var x in other)
            {
                Remove(x);
            }
        }

        public void SymmetricExceptWith(IEnumerable<T> other) => _source.SymmetricExceptWith(other);
        public bool IsSubsetOf(IEnumerable<T> other) => _source.IsSubsetOf(other);
        public bool IsSupersetOf(IEnumerable<T> other) => _source.IsSupersetOf(other);
        public bool IsProperSupersetOf(IEnumerable<T> other) => _source.IsProperSupersetOf(other);
        public bool IsProperSubsetOf(IEnumerable<T> other) => _source.IsProperSubsetOf(other);
        public bool Overlaps(IEnumerable<T> other) => _source.Overlaps(other);
        public bool SetEquals(IEnumerable<T> other) => _source.SetEquals(other);
        public void CopyTo(T[] array, int arrayIndex) => _source.CopyTo(array, arrayIndex);
        public IEnumerator<T> GetEnumerator() => _source.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => _source.GetEnumerator();

        public void Dispose()
        {
            _added.Dispose();
            _removed.Dispose();
            _cleared.Dispose();
            _source.Clear();
        }
    }
}