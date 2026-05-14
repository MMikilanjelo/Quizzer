using Source.Shared.Components.Elements.Label;
using UnityEngine;
using UnityEngine.UIElements;

namespace Source.Features.Quizzes.CreateQuiz.Components
{
    public class ModeSelectionTile : VisualElement
    {
        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                UpdateSelectionState();
            }
        }

        public ModeSelectionTile(string title, string subtitle , Sprite icon)
        {
            AddToClassList("mode-tile");
            var iconContainer = new VisualElement
            {
                style = { backgroundImage = new StyleBackground(icon) }
            };
            iconContainer.AddToClassList("mode-tile__icon");

            var titleLabel = new CustomLabel
            {
                Variant = CustomLabel.TextVariant.Regular,
                Weight = CustomLabel.FontWeight.SemiBold,
                text = title
            };
            titleLabel.AddToClassList("mode-tile__title");

            var subtitleLabel = new CustomLabel
            {
                Variant = CustomLabel.TextVariant.Small,
                Weight = CustomLabel.FontWeight.Regular,
                Alignment = TextAnchor.MiddleCenter,
                text = subtitle
            };
            subtitleLabel.AddToClassList("mode-tile__subtitle");

            Add(iconContainer);
            Add(titleLabel);
            Add(subtitleLabel);

            UpdateSelectionState();
        }

        private void UpdateSelectionState()
        {
            if (_isSelected)
            {
                AddToClassList("mode-tile--selected");
                RemoveFromClassList("mode-tile--unselected");
            }
            else
            {
                AddToClassList("mode-tile--unselected");
                RemoveFromClassList("mode-tile--selected");
            }
        }
    }
}