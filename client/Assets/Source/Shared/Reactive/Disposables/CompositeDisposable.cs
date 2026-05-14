using System;
using System.Collections.Generic;

namespace Source.Shared.Reactive.Disposables
{
    public sealed class CompositeDisposable : IDisposable
    {
        private readonly List<IDisposable> _disposables = new();

        private bool _disposed;

        public IDisposable Add(IDisposable item)
        {
            if (_disposed)
            {
                item?.Dispose();
                return this;
            }

            _disposables.Add(item);

            return this;
        }

        public void Remove(IDisposable item) =>
            _disposables.Remove(item);

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;

            foreach (var d in _disposables)
            {
                d?.Dispose();
            }

            _disposables.Clear();
        }
    }
}