using System;
using System.Collections.Generic;
using System.Linq;
using Source.Shared.Reactive;
using Source.Shared.Reactive.Disposables;
using Source.Shared.Reactive.Events;
using Source.Shared.Reactive.List;
using UnityEngine;
using UnityEngine.UIElements;

namespace Source.Shared.Components.List
{
    public class ReactiveVisualElementList<TModel> : VisualElement
    {
        public IReadOnlyReactiveEvent<TModel> ItemClicked => _itemClicked;

        private readonly ReactiveEvent<TModel> _itemClicked = new();

        private Func<VisualElement> _makeItem;
        private Action<VisualElement, TModel, CompositeDisposable> _bindItem;

        private readonly ScrollView _scrollView;
        private readonly VisualElement _container;

        private readonly Dictionary<TModel, ItemContext> _activeItems = new();
        private CompositeDisposable _listSubscriptions;

        private FlexDirection _direction = FlexDirection.Column;

        public Wrap Wrap
        {
            get => _container.style.flexWrap.value;
            set => _container.style.flexWrap = value;
        }

        public FlexDirection Direction
        {
            get => _direction;
            set
            {
                if (_direction == value)
                {
                    return;
                }

                _direction = value;

                _container.style.flexDirection = _direction;

                _scrollView.mode = _direction == FlexDirection.Row || _direction == FlexDirection.RowReverse
                    ? ScrollViewMode.Horizontal
                    : ScrollViewMode.Vertical;

                RefreshAllGaps();
            }
        }

        private float _columnGap;
        private float _rowGap;

        public float ColumnGap
        {
            get => _columnGap;
            set
            {
                if (Mathf.Approximately(_columnGap, value)) return;
                _columnGap = value;
                RefreshAllGaps();
            }
        }

        public float RowGap
        {
            get => _rowGap;
            set
            {
                if (Mathf.Approximately(_rowGap, value)) return;
                _rowGap = value;
                RefreshAllGaps();
            }
        }

        public float BottomPadding
        {
            get => _container?.style.paddingBottom.value.value ?? 0f;
            set => _container.style.paddingBottom = value;
        }

        private class ItemContext
        {
            public VisualElement Element { get; set; }
            public CompositeDisposable Disposables { get; set; }
        }

        public ReactiveVisualElementList()
        {
            AddToClassList("reactive-list");

            _scrollView = new ScrollView(ScrollViewMode.Vertical);
            _scrollView.AddToClassList("reactive-list__scroll-view");

            _container = _scrollView.contentContainer;
            _container.style.flexDirection = _direction;

            Add(_scrollView);

            RegisterCallback<DetachFromPanelEvent>(_ => ClearSubscriptions());
        }

        public void Bind<TVisualElement>(
            Func<TVisualElement> makeItem,
            Action<TVisualElement, TModel, CompositeDisposable> bindItem
        ) where TVisualElement : VisualElement
        {
            _makeItem = makeItem;
            _bindItem = (element, model, disposable) => bindItem((TVisualElement)element, model, disposable);
        }

        public void Set(IReadOnlyReactiveList<TModel> models)
        {
            ClearAll();
            ClearSubscriptions();

            _listSubscriptions = new CompositeDisposable();

            foreach (var model in models)
            {
                AddItem(model);
            }

            models.Added.Subscribe(AddItem).AddTo(_listSubscriptions);
            models.Removed.Subscribe(RemoveItem).AddTo(_listSubscriptions);
            models.Cleared.Subscribe(_ => ClearAll()).AddTo(_listSubscriptions);

            models.Replaced.Subscribe(evt =>
                {
                    RemoveItem(evt.OldValue);
                    AddItem(evt.NewValue);
                })
                .AddTo(_listSubscriptions);
        }

        private void AddItem(TModel model)
        {
            if (_activeItems.ContainsKey(model))
            {
                return;
            }

            var element = _makeItem();

            var itemDisposables = new CompositeDisposable();

            _bindItem?.Invoke(element, model, itemDisposables);

            var clickable = new Clickable(() => _itemClicked.Invoke(model));

            element.AddManipulator(clickable);

            itemDisposables.Add(new Disposable(() => element.RemoveManipulator(clickable)));

            var context = new ItemContext
            {
                Element = element,
                Disposables = itemDisposables
            };

            _container.Add(element);

            _activeItems.Add(model, context);

            RefreshAllGaps();
        }

        private void RemoveItem(TModel model)
        {
            if (!_activeItems.TryGetValue(model, out var context))
            {
                return;
            }

            context.Disposables.Dispose();

            _container.Remove(context.Element);

            _activeItems.Remove(model);

            RefreshAllGaps();
        }

        private void ClearAll()
        {
            foreach (var context in _activeItems.Select(kvp => kvp.Value))
            {
                context.Disposables.Dispose();
            }

            _container.Clear();
            _activeItems.Clear();
        }

        private void ApplyGapMargin(VisualElement element, bool isLastInList)
        {
            element.style.marginBottom = 0;
            element.style.marginTop = 0;
            element.style.marginLeft = 0;
            element.style.marginRight = 0;

            if (Wrap == Wrap.Wrap && !isLastInList)
            {
                element.style.marginBottom = _rowGap;
            }

            if (!isLastInList)
            {
                switch (_direction)
                {
                    case FlexDirection.Column:
                        element.style.marginBottom = _columnGap;
                        break;
                    case FlexDirection.ColumnReverse:
                        element.style.marginTop = _columnGap;
                        break;
                    case FlexDirection.Row:
                        element.style.marginRight = _columnGap;
                        break;
                    case FlexDirection.RowReverse:
                        element.style.marginLeft = _columnGap;
                        break;
                }
            }
        }

        private void RefreshAllGaps()
        {
            if (_container.childCount == 0) return;

            for (int i = 0; i < _container.childCount; i++)
            {
                bool isLast = (i == _container.childCount - 1);
                ApplyGapMargin(_container[i], isLast);
            }
        }

        private void ClearSubscriptions()
        {
            _listSubscriptions?.Dispose();
            _listSubscriptions = null;
        }
    }
}