using System;
using Source.Shared.Components.Elements.CustomVisualElement;
using Source.Shared.Components.Elements.Label;
using UnityEngine.UIElements;

namespace Source.Shared.Components.Elements.Chip
{
    [UxmlElement]
    public partial class Chip : ReactiveVisualElement
    {
        public enum ChipVariant
        {
            Outline,
            Filled
        }

        private readonly CustomLabel _label;

        private ChipVariant _variant = ChipVariant.Outline;

        [UxmlAttribute]
        public string Text
        {
            get => _label.text;
            set => _label.text = value;
        }

        [UxmlAttribute]
        public ChipVariant Variant
        {
            get => _variant;
            set
            {
                if (_variant == value)
                {
                    return;
                }

                _variant = value;
                RefreshStyles();
            }
        }

        private static readonly ChipVariant[] Variants =
            (ChipVariant[])Enum.GetValues(typeof(ChipVariant));

        public Chip()
        {
            _label = new CustomLabel
            {
                Variant = CustomLabel.TextVariant.Regular,
                Weight = CustomLabel.FontWeight.Regular,
                pickingMode = PickingMode.Ignore
            };

            _label.AddToClassList("chip__text");

            Add(_label);

            RefreshStyles();
        }

        private void RefreshStyles()
        {
            foreach (var v in Variants)
            {
                RemoveFromClassList($"chip--{v.ToString().ToLower()}");
            }

            AddToClassList($"chip--{Variant.ToString().ToLower()}");
        }
    }
}