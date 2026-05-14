using Source.Shared.Components.Elements.Button;
using UnityEngine.UIElements;

namespace Source.Shared.Components.Layouts.ActionFooter
{
    public class ActionFooter : VisualElement
    {
        private readonly VisualElement _content;

        public ActionFooter()
        {
            pickingMode = PickingMode.Ignore;
            AddToClassList("action-footer");

            _content = new VisualElement();
            _content.AddToClassList("action-footer__content");
            _content.pickingMode = PickingMode.Ignore;
            Add(_content);
        }

        public void AddAction(CustomButton button)
        {
            button.AddToClassList("action-footer__item");
            _content.Add(button);
        }

        public void ClearButtons()
        {
            foreach (var child in _content.Children())
            {
                child.RemoveFromClassList("action-footer__item");
            }

            _content.Clear();
        }
    }
}