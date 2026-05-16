using System;
using Source.Shared.Components.Elements.CustomVisualElement;
using Source.Shared.Components.Elements.Label;
using Unity.VectorGraphics;
using UnityEngine.UIElements;

namespace Source.Shared.Components.Elements.Badge
{
    public partial class Badge : ReactiveVisualElement
    {
        public enum BadgeVariant
        {
            Success,
            Info,
            Neutral
        }

        public enum BadgeSize
        {
            Small,
            Medium,
            Large
        }

        public enum BadgeShape
        {
            Pill,
            Rounded,
        }

        private readonly CustomLabel _label;

        private BadgeVariant _variant = BadgeVariant.Success;
        private BadgeSize _size = BadgeSize.Medium;
        private BadgeShape _shape = BadgeShape.Pill;

        [UxmlAttribute]
        public string Text
        {
            get => _label.text;
            set => _label.text = value;
        }

        [UxmlAttribute]
        public BadgeVariant Variant
        {
            get => _variant;
            set
            {
                if (_variant == value) return;
                _variant = value;
                RefreshStyles();
            }
        }

        [UxmlAttribute]
        public BadgeSize Size
        {
            get => _size;
            set
            {
                if (_size == value) return;
                _size = value;
                RefreshStyles();
            }
        }

        [UxmlAttribute]
        public BadgeShape Shape
        {
            get => _shape;
            set
            {
                if (_shape == value) return;
                _shape = value;
                RefreshStyles();
            }
        }

        private static readonly BadgeVariant[] Variants = (BadgeVariant[])Enum.GetValues(typeof(BadgeVariant));
        private static readonly BadgeSize[] Sizes = (BadgeSize[])Enum.GetValues(typeof(BadgeSize));
        private static readonly BadgeShape[] Shapes = (BadgeShape[])Enum.GetValues(typeof(BadgeShape));

        public Badge()
        {
            AddToClassList("badge");

            _label = new CustomLabel
            {
                Weight = CustomLabel.FontWeight.Regular,
                pickingMode = PickingMode.Ignore
            };

            _label.AddToClassList("badge__text");

            Add(_label);

            RefreshStyles();
        }

        private void RefreshStyles()
        {
            foreach (var v in Variants)
            {
                RemoveFromClassList($"badge--{v.ToString().ToLower()}");
            }

            foreach (var s in Sizes)
            {
                RemoveFromClassList($"badge--{s.ToString().ToLower()}");
            }

            foreach (var sh in Shapes)
            {
                RemoveFromClassList($"badge--{sh.ToString().ToLower()}");
            }

            AddToClassList($"badge--{Variant.ToString().ToLower()}");
            AddToClassList($"badge--{Size.ToString().ToLower()}");
            AddToClassList($"badge--{Shape.ToString().ToLower()}");

            _label.Variant = Size switch
            {
                BadgeSize.Small => CustomLabel.TextVariant.Tiny,
                BadgeSize.Medium => CustomLabel.TextVariant.Small,
                BadgeSize.Large => CustomLabel.TextVariant.Regular,
                _ => CustomLabel.TextVariant.Small
            };
        }
    }
}