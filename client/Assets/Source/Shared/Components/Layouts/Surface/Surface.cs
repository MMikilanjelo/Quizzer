using UnityEngine.UIElements;

namespace Source.Shared.Components.Layouts.Surface
{
    [UxmlElement]
    public partial class Surface : VisualElement
    {
        public enum Spacing
        {
            None,
            Xxs,
            Xs,
            S,
            Sm,
            M,
            Md,
            L,
            Xl,
            Xxl,
            ThreeXl
        }

        private Spacing _paddingTop = Spacing.M;
        private Spacing _paddingBottom = Spacing.M;
        private Spacing _paddingLeft = Spacing.M;
        private Spacing _paddingRight = Spacing.M;

        [UxmlAttribute]
        public Spacing PaddingAll
        {
            get => _paddingTop;
            set
            {
                PaddingTop = value;
                PaddingBottom = value;
                PaddingLeft = value;
                PaddingRight = value;
            }
        }

        [UxmlAttribute]
        public Spacing PaddingTop
        {
            get => _paddingTop;
            set
            {
                if (_paddingTop == value) return;
                UpdatePaddingClass("pt", _paddingTop, value);
                _paddingTop = value;
            }
        }

        [UxmlAttribute]
        public Spacing PaddingBottom
        {
            get => _paddingBottom;
            set
            {
                if (_paddingBottom == value) return;
                UpdatePaddingClass("pb", _paddingBottom, value);
                _paddingBottom = value;
            }
        }

        [UxmlAttribute]
        public Spacing PaddingLeft
        {
            get => _paddingLeft;
            set
            {
                if (_paddingLeft == value) return;
                UpdatePaddingClass("pl", _paddingLeft, value);
                _paddingLeft = value;
            }
        }

        [UxmlAttribute]
        public Spacing PaddingRight
        {
            get => _paddingRight;
            set
            {
                if (_paddingRight == value) return;
                UpdatePaddingClass("pr", _paddingRight, value);
                _paddingRight = value;
            }
        }

        public Surface()
        {
            AddToClassList("surface");
            AddToClassList($"surface--pt-{_paddingTop.ToString().ToLower()}");
            AddToClassList($"surface--pb-{_paddingBottom.ToString().ToLower()}");
            AddToClassList($"surface--pl-{_paddingLeft.ToString().ToLower()}");
            AddToClassList($"surface--pr-{_paddingRight.ToString().ToLower()}");
        }

        private void UpdatePaddingClass(string prefix, Spacing oldSpacing, Spacing newSpacing)
        {
            RemoveFromClassList($"surface--{prefix}-{oldSpacing.ToString().ToLower()}");
            AddToClassList($"surface--{prefix}-{newSpacing.ToString().ToLower()}");
        }
    }
}