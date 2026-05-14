using UnityEngine.UIElements;

namespace Source.Shared.Components.Layouts.SafeArea
{
    [UxmlElement]
    public partial class SafeAreaContainer : VisualElement
    {
        public SafeAreaContainer()
        {
            style.flexGrow = 1;
            style.width = new Length(100, LengthUnit.Percent);
            style.height = new Length(100, LengthUnit.Percent);

            RegisterCallback<AttachToPanelEvent>(OnAttach);
            RegisterCallback<DetachFromPanelEvent>(OnDetach);
        }

        private void OnAttach(AttachToPanelEvent evt)
        {
            RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
            ApplySafeArea();
        }

        private void OnDetach(DetachFromPanelEvent evt)
        {
            UnregisterCallback<GeometryChangedEvent>(OnGeometryChanged);
        }

        private void OnGeometryChanged(GeometryChangedEvent evt) => ApplySafeArea();

        private void ApplySafeArea()
        {
            if (panel == null)
            {
                return;
            }

            var scaleFactor = panel.scaledPixelsPerPoint;

            var safeArea = UnityEngine.Screen.safeArea;

            var left = safeArea.x / scaleFactor;
            var top = (UnityEngine.Screen.height - (safeArea.y + safeArea.height)) / scaleFactor;
            var right = (UnityEngine.Screen.width - (safeArea.x + safeArea.width)) / scaleFactor;

            style.paddingLeft = left;
            style.paddingTop = top;
            style.paddingRight = right;
        }
    }
}