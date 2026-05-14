using System;
using Source.Shared.Reactive.Disposables;

namespace Source.Shared.Reactive
{
    public static class ReactiveExtensions
    {
        public static void AddTo(this IDisposable disposable, CompositeDisposable compositeDisposable) =>
            compositeDisposable.Add(disposable);
    }
}