using Source.Shared.Components.Elements.Button;
using UnityEngine;
using UnityEngine.UIElements;

namespace Source.Shared.Components.Headers.FlowHeader
{
    public class FlowHeader : VisualElement
    {
        public CustomButton BackButton { get; }
        public ProgressDots.ProgressDots Dots { get; }

        public FlowHeader(Sprite backIcon)
        {
            AddToClassList("flow-header");

            var leftSlot = new VisualElement();
            leftSlot.AddToClassList("flow-header__side-slot");

            BackButton = new CustomButton
            {
                name = "BackButton",
                Variant = CustomButton.ButtonVariant.Ghost,
                Size = CustomButton.ButtonSize.Small,
                Shape = CustomButton.ButtonShape.Circle,
                Text = string.Empty,
                Icon = backIcon
            };
            BackButton.AddToClassList("flow-header__back-button");

            leftSlot.Add(BackButton);
            Add(leftSlot);

            Dots = new ProgressDots.ProgressDots();
            Dots.AddToClassList("flow-header__center-slot");
            Add(Dots);

            var rightSlot = new VisualElement();
            rightSlot.AddToClassList("flow-header__side-slot");
            rightSlot.style.alignItems = Align.FlexEnd;

            Add(rightSlot);
        }
    }
}