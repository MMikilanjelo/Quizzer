using UnityEngine.UIElements;

namespace Source.Shared.Components.Elements.ProgressBar
{
    [UxmlElement]
    public partial class CustomProgressBar : UnityEngine.UIElements.ProgressBar
    {
        public enum ProgressBarVariant
        {
            Primary,
            Success,
            Warning,
            Danger
        }

        private ProgressBarVariant _variant;

        public ProgressBarVariant Variant
        {
            get => _variant;
            set
            {
                RemoveFromClassList("custom-progress-bar--primary");
                RemoveFromClassList("custom-progress-bar--success");
                RemoveFromClassList("custom-progress-bar--warning");
                RemoveFromClassList("custom-progress-bar--danger");

                _variant = value;

                AddToClassList($"custom-progress-bar--{value.ToString().ToLower()}");
            }
        }

        public CustomProgressBar()
        {
            AddToClassList("custom-progress-bar");
            Variant = ProgressBarVariant.Primary; 
            title = string.Empty;
            lowValue = 0f;
            highValue = 1f;
        }

        public void SetNormalizedValue(float percent)
        {
            value = UnityEngine.Mathf.Clamp01(percent);
        }
    }
}