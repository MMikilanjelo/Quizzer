using System;
using System.Collections.Generic;
using System.Linq;
using Source.Shared.Components.Elements.CustomVisualElement;
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

        private Func<ReactiveVisualElement> _makeItem;
        private Action<ReactiveVisualElement, TModel> _bindItem;

        private readonly ScrollView _scrollView;
        private readonly VisualElement _container;

        private readonly Dictionary<TModel, ReactiveVisualElement> _activeItems = new();
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
            Action<TVisualElement, TModel> bindItem
        ) where TVisualElement : ReactiveVisualElement
        {
            _makeItem = makeItem;
            _bindItem = (element, model) => bindItem((TVisualElement)element, model);
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

            _bindItem?.Invoke(element, model);

            var clickable = new Clickable(() => _itemClicked.Invoke(model));

            element.AddManipulator(clickable);

            element.Disposables.Add(new Disposable(() => element.RemoveManipulator(clickable)));

            _container.Add(element);

            _activeItems.Add(model, element);

            RefreshAllGaps();
        }

        private void RemoveItem(TModel model)
        {
            if (!_activeItems.TryGetValue(model, out var item))
            {
                return;
            }

            item.Dispose();

            _container.Remove(item);

            _activeItems.Remove(model);

            RefreshAllGaps();
        }

        private void ClearAll()
        {
            foreach (var item in _activeItems.Select(kvp => kvp.Value))
            {
                item.Dispose();
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

    public class ReactiveVisualElementList : VisualElement
    {
        private readonly ScrollView _scrollView;
        private readonly VisualElement _container;

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

        public ReactiveVisualElementList()
        {
            AddToClassList("reactive-list");

            _scrollView = new ScrollView(ScrollViewMode.Vertical);
            _scrollView.AddToClassList("reactive-list__scroll-view");

            _container = _scrollView.contentContainer;
            _container.style.flexDirection = _direction;

            hierarchy.Add(_scrollView);
        }

        public void AddItem(VisualElement element)
        {
            _container.Add(element);
            RefreshAllGaps();
        }

        public void RemoveItem(VisualElement element)
        {
            _container.Remove(element);
            RefreshAllGaps();
        }

        public void ClearAll()
        {
            _container.Clear();
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
    }
}