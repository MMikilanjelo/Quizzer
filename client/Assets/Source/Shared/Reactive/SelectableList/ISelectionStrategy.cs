using System.Collections.Generic;
using System.Linq;

namespace Source.Shared.Reactive.SelectableList
{
    public interface ISelectionStrategy<in T> where T : ISelectable
    {
        void Initialize(IEnumerable<T> initialItems)
        {
        }

        void Execute(T clickedItem, IEnumerable<T> allItems);
        bool IsValid(IEnumerable<T> allItems);
    }

    public class ExclusiveStrategy<T> : ISelectionStrategy<T> where T : ISelectable
    {
        public void Execute(T clickedItem, IEnumerable<T> allItems)
        {
            if (clickedItem.IsSelected.Value) return;

            foreach (var item in allItems) item.Deselect();
            clickedItem.Select();
        }

        public bool IsValid(IEnumerable<T> allItems) => allItems.Any(i => i.IsSelected.Value);
    }

    public class MultiStrategy<T> : ISelectionStrategy<T> where T : ISelectable
    {
        public void Execute(T clickedItem, IEnumerable<T> allItems)
        {
            if (clickedItem.IsSelected.Value) clickedItem.Deselect();
            else clickedItem.Select();
        }

        public bool IsValid(IEnumerable<T> allItems) => allItems.Any(i => i.IsSelected.Value);
    }

    public class CappedStrategy<T> : ISelectionStrategy<T> where T : ISelectable
    {
        private readonly int _min;
        private readonly int _max;
        private readonly List<T> _selectionHistory = new();

        public CappedStrategy(int min, int max)
        {
            _min = min;
            _max = max;
        }

        public void Initialize(IEnumerable<T> initialItems)
        {
            _selectionHistory.Clear();
            _selectionHistory.AddRange(initialItems.Where(i => i.IsSelected.Value));
        }

        public void Execute(T clickedItem, IEnumerable<T> allItems)
        {
            if (clickedItem.IsSelected.Value)
            {
                clickedItem.Deselect();
                _selectionHistory.Remove(clickedItem);
            }
            else
            {
                var selectables = allItems as List<T> ?? allItems.ToList();

                if (selectables.Count(i => i.IsSelected.Value) >= _max)
                {
                    var lastSelectedItem = _selectionHistory.LastOrDefault()
                                           ?? selectables.LastOrDefault(i => i.IsSelected.Value);

                    if (lastSelectedItem != null)
                    {
                        lastSelectedItem.Deselect();
                        
                        _selectionHistory.Remove(lastSelectedItem);
                    }
                }

                clickedItem.Select();
                _selectionHistory.Add(clickedItem);
            }
        }

        public bool IsValid(IEnumerable<T> allItems)
        {
            var count = allItems.Count(i => i.IsSelected.Value);
            return count >= _min && count <= _max;
        }
    }
}