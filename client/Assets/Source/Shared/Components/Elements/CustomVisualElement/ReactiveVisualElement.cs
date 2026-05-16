using System;
using Source.Shared.Reactive.Disposables;
using UnityEngine.UIElements;

namespace Source.Shared.Components.Elements.CustomVisualElement
{
    public class ReactiveVisualElement : VisualElement, IDisposable
    {
        private CompositeDisposable _disposables;
        public CompositeDisposable Disposables => _disposables ??= new CompositeDisposable();

        protected ReactiveVisualElement() =>
            RegisterCallback<DetachFromPanelEvent>(_ => Dispose());

        public void Dispose()
        {
            _disposables?.Dispose();
            _disposables = null;
        }
    }
}