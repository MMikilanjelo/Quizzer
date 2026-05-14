using Source.Shared.Reactive.Events;
using Source.Shared.Reactive.SelectableList;

namespace Source.Features.Quizzes.CreateQuiz.ViewModels
{
    public class SelectDomainItemViewModel : ISelectable
    {
        public string Name { get; private set; }
        public IReadOnlyReactiveProperty<bool> IsSelected => _isSelected;

        private readonly ReactiveProperty<bool> _isSelected = new(false);

        public SelectDomainItemViewModel(string name)
        {
            Name = name;
        }

        public void Select() =>
            _isSelected.Value = true;

        public void Deselect() =>
            _isSelected.Value = false;
    }
}