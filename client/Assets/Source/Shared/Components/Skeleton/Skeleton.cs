using UnityEngine.UIElements;

namespace Source.Shared.Components.Skeleton
{
    [UxmlElement]
    public partial class Skeleton : VisualElement
    {
        public enum SkeletonVariant
        {
            Block,
            Avatar,
            Thumbnail,
            Title1,
            Title2,
            Title3,
            Large,
            Regular,
            Small,
            Tiny
        }

        private IVisualElementScheduledItem _pulseTask;
        private bool _isDimmed;
        private SkeletonVariant _variant = SkeletonVariant.Block; 

        [UxmlAttribute] public bool AutoStart { get; set; } = true;

        [UxmlAttribute]
        public SkeletonVariant Variant
        {
            get => _variant;
            set
            {
                RemoveFromClassList($"skeleton--{_variant.ToString().ToLower()}");
                _variant = value;
                AddToClassList($"skeleton--{_variant.ToString().ToLower()}");
            }
        }

        public Skeleton()
        {
            AddToClassList("skeleton");
            AddToClassList($"skeleton--{_variant.ToString().ToLower()}");

            RegisterCallback<AttachToPanelEvent>(OnAttach);
            RegisterCallback<DetachFromPanelEvent>(OnDetach);
        }

        private void OnAttach(AttachToPanelEvent evt)
        {
            if (AutoStart)
            {
                StartAnimation();
            }
        }

        private void OnDetach(DetachFromPanelEvent evt) => StopAnimation();

        public void StartAnimation()
        {
            if (_pulseTask is { isActive: true }) return;
            _pulseTask = schedule.Execute(ToggleState).Every(1000);
        }

        public void StopAnimation()
        {
            _pulseTask?.Pause();
            RemoveFromClassList("skeleton--dimmed");
            _isDimmed = false;
        }

        private void ToggleState()
        {
            _isDimmed = !_isDimmed;
            ToggleInClassList("skeleton--dimmed");
        }
    }
}