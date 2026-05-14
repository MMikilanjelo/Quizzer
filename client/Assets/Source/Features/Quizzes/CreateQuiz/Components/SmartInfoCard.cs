using Source.Shared.Components.Elements.Label;
using UnityEngine;
using UnityEngine.UIElements;

namespace Source.Features.Quizzes.CreateQuiz.Components
{
    public class SmartInfoCard : VisualElement
    {
        public SmartInfoCard(Sprite icon)
        {
            AddToClassList("smart-info-card");

            var iconElement = new VisualElement();
            iconElement.AddToClassList("smart-info-card__icon");
            iconElement.style.backgroundImage = new StyleBackground(icon);

            var contentColumn = new VisualElement();
            contentColumn.AddToClassList("smart-info-card__content");

            var titleLabel = new CustomLabel
            {
                Variant = CustomLabel.TextVariant.Small,
                Weight = CustomLabel.FontWeight.SemiBold,
                text = "We will select topics based on:"
            };
            titleLabel.AddToClassList("smart-info-card__title");

            contentColumn.Add(titleLabel);
            contentColumn.Add(CreateBulletRow("Your learning history"));
            contentColumn.Add(CreateBulletRow("Areas needing improvement"));
            contentColumn.Add(CreateBulletRow("Recommended practice topics"));

            Add(iconElement);
            Add(contentColumn);
        }

        private static VisualElement CreateBulletRow(string message)
        {
            var row = new VisualElement();
            row.AddToClassList("smart-info-card__bullet-row");

            var dot = new VisualElement();
            dot.AddToClassList("smart-info-card__bullet-dot");

            var label = new CustomLabel
            {
                Variant = CustomLabel.TextVariant.Small,
                Weight = CustomLabel.FontWeight.Regular,
                text = message
            };
            label.AddToClassList("smart-info-card__bullet-text");

            row.Add(dot);
            row.Add(label);
            return row;
        }
    }
}