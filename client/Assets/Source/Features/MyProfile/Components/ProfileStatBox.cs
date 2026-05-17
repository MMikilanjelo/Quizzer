using Source.Shared.Components.Elements.Label;
using Source.Shared.Components.Skeleton;
using UnityEngine;
using UnityEngine.UIElements;

namespace Source.Features.MyProfile.Components
{
    public class ProfileStatBox : VisualElement
    {
        private readonly CustomLabel _valueLabel;

        public ProfileStatBox(string labelText)
        {
            AddToClassList("my-profile__stat-box");

            _valueLabel = new CustomLabel
            {
                text = "-",
                Variant = CustomLabel.TextVariant.Title3,
                Weight = CustomLabel.FontWeight.Bold
            };
            _valueLabel.AddToClassList("my-profile__stat-value");

            var titleLabel = new CustomLabel
            {
                text = labelText,
                Variant = CustomLabel.TextVariant.Small,
                Weight = CustomLabel.FontWeight.Regular,
                Alignment = TextAnchor.MiddleCenter
            };
            titleLabel.AddToClassList("my-profile__stat-label");

            Add(_valueLabel);
            Add(titleLabel);
        }

        public void SetValue(string value) =>
            _valueLabel.text = value;
    }

    public class ProfileStatBoxSkeleton : VisualElement
    {
        public ProfileStatBoxSkeleton()
        {
            AddToClassList("my-profile__stat-box");
            var valueSkeleton = new Skeleton
            {
                Variant = Skeleton.SkeletonVariant.Title2,
                style =
                {
                    width = Length.Percent(50),
                    marginBottom = 8
                }
            };

            var labelSkeleton = new Skeleton
            {
                Variant = Skeleton.SkeletonVariant.Small,
                style =
                {
                    width = Length.Percent(80)
                }
            };

            Add(valueSkeleton);
            Add(labelSkeleton);
        }
    }
}