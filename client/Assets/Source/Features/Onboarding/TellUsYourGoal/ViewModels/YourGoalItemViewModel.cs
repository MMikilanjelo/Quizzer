using Cysharp.Threading.Tasks;
using Source.Features.Onboarding.TellUsYourGoal.Models;
using Source.Shared.Reactive.Events;
using Source.Shared.Reactive.SelectableList;

namespace Source.Features.Onboarding.TellUsYourGoal.ViewModels
{
    public class YourGoalItemViewModel : ISelectable
    {
        public YourGoalModel Model { get; }
        public string Id => Model.Id;
        public string Name => Model.Name;

        public IReadOnlyReactiveProperty<bool> IsSelected => _isSelected;

        private readonly ReactiveProperty<bool> _isSelected;

        public YourGoalItemViewModel(YourGoalModel model, bool isSelected = false)
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