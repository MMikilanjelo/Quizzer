using Cysharp.Threading.Tasks;
using Source.Features.Onboarding.Models;
using Source.Shared.Reactive.Commands;
using Source.Shared.Reactive.Events;
using Source.Shared.Reactive.SelectableList;

namespace Source.Features.Onboarding.TellUsYourInterests.ViewModels
{
    public class YourInterestItemViewModel : ISelectable
    {
        public IReadOnlyReactiveProperty<bool> IsSelected => _isSelected;

        public string Id => Model.Id;
        public string Name => Model.Name;
        public YourInterestModel Model { get; }

        private readonly ReactiveProperty<bool> _isSelected = new(false);

        public YourInterestItemViewModel(YourInterestModel model) =>
            Model = model;

        public void Select() =>
            _isSelected.Value = true;

        public void Deselect() =>
            _isSelected.Value = false;
    }
}