using Source.Shared.Reactive.Events;
using Source.Shared.Reactive.SelectableList;

namespace Source.Features.Quizzes.ActiveQuiz.VIewModels
{
    public class QuizOptionViewModel : ISelectable
    {
        public string Text { get; }
        public string LetterIndicator { get; }
        public IReadOnlyReactiveProperty<bool> IsSelected => _isSelected;

        private readonly ReactiveProperty<bool> _isSelected = new();

        public QuizOptionViewModel(string text, string letterIndicator)
        {
            Text = text;
            LetterIndicator = letterIndicator;
        }

        public void Select() =>
            _isSelected.Value = true;

        public void Deselect() =>
            _isSelected.Value = false;
    }
}