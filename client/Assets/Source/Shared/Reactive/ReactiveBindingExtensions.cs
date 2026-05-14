using System;
using Source.Shared.Reactive.Disposables;
using Source.Shared.Reactive.Events;
using UnityEngine.UIElements;

namespace Source.Shared.Reactive
{
    public static class ReactiveBindingExtensions
    {
        public static IDisposable BindProperty<TElement, TValue>(
            this TElement element,
            IReactiveProperty<TValue> source,
            Action<TElement, TValue> setter
        ) where TElement : VisualElement
        {
            return source.Subscribe(value => setter(element, value));
        }

        public static IDisposable BindProperty<TElement, TValue>(
            this TElement element,
            IReadOnlyReactiveProperty<TValue> source,
            Action<TElement, TValue> setter
        ) where TElement : VisualElement
        {
            return source.Subscribe(value => setter(element, value));
        }
        public static IDisposable RegisterDisposableCallback<T>(this VisualElement element, EventCallback<T> callback) 
            where T : EventBase<T>, new()
        {
            element.RegisterCallback(callback);
            return new Disposable(() => element.UnregisterCallback(callback));
        }
    }
}