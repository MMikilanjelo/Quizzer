using UnityEngine.UIElements;

namespace Source.Shared.Components.Elements.Backdrop
{
    [UxmlElement]
    public partial class Backdrop : VisualElement
    {
        public Backdrop()
        {
            AddToClassList("backdrop");
            pickingMode = PickingMode.Ignore;
        }

        public void Show()
        {
            pickingMode = PickingMode.Position;
            AddToClassList("backdrop--active");
        }

        public void Hide()
        {
            pickingMode = PickingMode.Ignore;
            RemoveFromClassList("backdrop--active");
        }
    }
}