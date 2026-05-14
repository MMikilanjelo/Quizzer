using System;
using Source.Features.Quizzes.MyQuizzes.Models;
using Source.Shared.Reactive.Events;
using Source.Shared.Reactive.SelectableList;

namespace Source.Features.Quizzes.MyQuizzes.ViewModels
{
    public class QuizzesStateFilterItemViewModel : ISelectable
    {
        public QuizFilterModel Model { get; }
        public string Name => Model.Filter switch
        {
            QuizFilter.All => "All",
            QuizFilter.Pending => "Pending",
            QuizFilter.Active => "Active",
            QuizFilter.Completed => "Completed",
            _ => string.Empty
        };

        public IReadOnlyReactiveProperty<bool> IsSelected => _isSelected;

        private readonly ReactiveProperty<bool> _isSelected;

        public QuizzesStateFilterItemViewModel(QuizFilterModel model, bool isSelected = false)
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