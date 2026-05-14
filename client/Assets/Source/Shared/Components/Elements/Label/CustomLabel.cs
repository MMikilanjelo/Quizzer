using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Source.Shared.Components.Elements.Label
{
    [UxmlElement]
    public partial class CustomLabel : UnityEngine.UIElements.Label
    {
        public enum TextVariant
        {
            Title1,
            Title2,
            Title3,
            Large,
            Regular,
            Small,
            Tiny
        }


        public enum FontWeight
        {
            Regular = 400,
            Medium = 500,
            SemiBold = 600,
            Bold = 700
        }
        
        private TextVariant _variant = TextVariant.Regular;

        [UxmlAttribute]
        public TextVariant Variant
        {
            get => _variant;
            set
            {
                _variant = value;
                UpdateStyling();
            }
        }

        private FontWeight _weight = FontWeight.Regular;

        [UxmlAttribute]
        public FontWeight Weight
        {
            get => _weight;
            set
            {
                _weight = value;
                UpdateStyling();
            }
        }

        private string _rawText = string.Empty;

        public override string text
        {
            get => _rawText;
            set
            {
                if (_rawText == value)
                {
                    return;
                }

                _rawText = value;

                UpdateStyling();
            }
        }

        [UxmlAttribute]
        public TextAnchor Alignment
        {
            get => _alignment;
            set
            {
                _alignment = value;
                style.unityTextAlign = _alignment; 
            }
        }

        private TextAnchor _alignment = TextAnchor.UpperLeft;

        public CustomLabel()
        {
            AddToClassList("custom-label");
            UpdateStyling();
        }

        private void UpdateStyling()
        {
            foreach (TextVariant v in Enum.GetValues(typeof(TextVariant)))
            {
                var className = $"custom-label--variant-{v.ToString().ToLower()}";

                if (ClassListContains(className))
                {
                    RemoveFromClassList(className);
                }
            }

            AddToClassList($"custom-label--variant-{_variant.ToString().ToLower()}");

            base.text = $"<font-weight={(int)_weight}>{_rawText}</font-weight>";

            MarkDirtyRepaint();
        }
    }
}