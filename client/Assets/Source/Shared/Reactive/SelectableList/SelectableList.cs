using System.Collections.Generic;
using System.Linq;
using Source.Shared.Reactive.Commands;
using Source.Shared.Reactive.Events;
using Source.Shared.Reactive.List;

namespace Source.Shared.Reactive.SelectableList
{
    public class SelectableList<T> where T : ISelectable
    {
        public IReadOnlyReactiveList<T> Items => _items;
        public IReadOnlyReactiveProperty<bool> IsValid => _isValid;
        public IReadOnlyReactiveEvent<T> ItemSelectionChanged => _itemSelectionChanged;
        public ICommand<T> SelectCommand { get; }

        private readonly ReactiveList<T> _items = new();
        private readonly ReactiveProperty<bool> _isValid = new(false);
        private readonly ReactiveEvent<T> _itemSelectionChanged = new();

        private readonly ISelectionStrategy<T> _strategy;

        private SelectableList(ISelectionStrategy<T> initialStrategy)
        {
            _strategy = initialStrategy;
            SelectCommand = SyncCommand<T>.Create(Select);
        }

        public void Set(IEnumerable<T> items)
        {
            _items.Clear();
            _items.AddRange(items);
            _strategy.Initialize(_items);
            Validate();
        }

        private void Select(T item)
        {
            var previousStates = _items.ToDictionary(i => i, i => i.IsSelected.Value);

            _strategy.Execute(item, _items);

            Validate();

            foreach (var currentItem in _items)
            {
                var wasSelected = previousStates[currentItem];

                if (wasSelected != currentItem.IsSelected.Value)
                {
                    _itemSelectionChanged.Invoke(currentItem);
                }
            }
        }

        private void Validate() =>
            _isValid.Value = _strategy.IsValid(_items);

        public void Dispose()
        {
            _items.Dispose();

            _isValid.Dispose();

            _itemSelectionChanged.Dispose();
        }

        public static SelectableList<T> Exclusive() =>
            new(new ExclusiveStrategy<T>());

        public static SelectableList<T> Multi() =>
            new(new MultiStrategy<T>());

        public static SelectableList<T> Capped(int min, int max) =>
            new(new CappedStrategy<T>(min, max));
    }
}