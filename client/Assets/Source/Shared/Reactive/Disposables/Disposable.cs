using System;

namespace Source.Shared.Reactive.Disposables
{
    public sealed class Disposable : IDisposable
    {
        public Disposable(Action disposeAction)
        {
            _disposeAction = disposeAction;
        }

        public static IDisposable Empty { get; private set; } = new Disposable(() => { });

        private Action _disposeAction;

        private bool _disposed;

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;

            _disposeAction?.Invoke();

            _disposeAction = null;
        }
    }
}