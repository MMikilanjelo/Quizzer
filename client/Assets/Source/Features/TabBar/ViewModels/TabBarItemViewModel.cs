using Source.Features.TabBar.Models;
using Source.Shared.Reactive.Events;
using Source.Shared.Reactive.SelectableList;

namespace Source.Features.TabBar.ViewModels
{
    public class TabBarItemViewModel : ISelectable
    {
        public TabItemModel Model { get; }
        public IReadOnlyReactiveProperty<bool> IsSelected => _isSelected;
        
        private readonly ReactiveProperty<bool> _isSelected;

        public TabBarItemViewModel(TabItemModel model, bool isSelected = false)
        {
            Model = model;
            _isSelected = new ReactiveProperty<bool>(isSelected);
        }

        public void Select() =>
            _isSelected.Value = true;

        public void Deselect() =>
            _isSelected.Value = false;
    }
}