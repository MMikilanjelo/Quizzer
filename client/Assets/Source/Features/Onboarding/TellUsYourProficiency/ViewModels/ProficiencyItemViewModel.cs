using Source.Features.Onboarding.Models;
using Source.Shared.Reactive.Events;
using Source.Shared.Reactive.SelectableList;

namespace Source.Features.Onboarding.TellUsYourProficiency.ViewModels
{
    public class ProficiencyItemViewModel : ISelectable
    {
        public IReadOnlyReactiveProperty<bool> IsSelected => _isSelected;

        public string Id => _model.Id;
        
        public string Name => _model.Name;

        private readonly ReactiveProperty<bool> _isSelected = new(false);

        private readonly ProficiencyModel _model;

        public ProficiencyItemViewModel(ProficiencyModel model) =>
            _model = model;

        public void Select() =>
            _isSelected.Value = true;

        public void Deselect() =>
            _isSelected.Value = false;
    }
}