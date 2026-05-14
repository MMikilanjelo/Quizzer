using Source.Shared.Reactive.Events;

namespace Source.Shared.Reactive.SelectableList
{
    public interface ISelectable
    {
        IReadOnlyReactiveProperty<bool> IsSelected { get; }
        void Select();
        void Deselect();
    }
}