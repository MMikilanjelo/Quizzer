using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UIElements;

namespace Source.Shared.Components.Dialogs
{
    public abstract class Dialog : IDialogView
    {
        public VisualElement Root { get; }
        protected VisualElement Card { get; set; }

        private Sequence _animationSequence;
        private const float ShowDuration = 0.5f;
        private const float HideDuration = 0.25f;
        private const float SpringOvershoot = 1.2f;

        protected Dialog(VisualElement root)
        {
            Root = root;
            Root.style.position = Position.Absolute;
            Root.style.width = new Length(100, LengthUnit.Percent);
            Root.style.height = new Length(100, LengthUnit.Percent);

            Root.style.justifyContent = Justify.Center;
            Root.style.alignItems = Align.Center;
        }

        public abstract void Dispose();

        public async UniTask Show()
        {
            Root.visible = true;

            Card.style.opacity = 0f;
            Card.style.scale = new Scale(new Vector2(0.88f, 0.88f));
            Card.style.translate = new Translate(0, 60f, 0);
            Card.style.rotate = new Rotate(Angle.Degrees(-5f));
            Card.style.transformOrigin = new TransformOrigin(Length.Percent(50), Length.Percent(50));

            _animationSequence?.Kill();
            _animationSequence = DOTween.Sequence();

            _animationSequence.Join(DOTween.To(() => Card.style.opacity.value,
                    x => Card.style.opacity = x, 1f, ShowDuration * 0.4f)
                .SetEase(Ease.OutCubic));

            _animationSequence.Join(DOTween.To(() => Card.style.translate.value.y.value,
                    y => Card.style.translate = new Translate(0, y, 0), 0f, ShowDuration)
                .SetEase(Ease.OutBack, SpringOvershoot));

            _animationSequence.Join(DOTween.To(() => Card.style.scale.value.value.x,
                    s => Card.style.scale = new Scale(new Vector2(s, s)), 1f, ShowDuration)
                .SetEase(Ease.OutBack, SpringOvershoot));

            _animationSequence.Join(DOTween.To(() => Card.style.rotate.value.angle.value,
                    r => Card.style.rotate = new Rotate(Angle.Degrees(r)), 0f, ShowDuration)
                .SetEase(Ease.OutQuint));

            await _animationSequence.Play().ToUniTask();
        }

        public async UniTask Hide()
        {
            _animationSequence?.Kill();
            _animationSequence = DOTween.Sequence();

            _animationSequence.Join(DOTween.To(() => Card.style.opacity.value,
                    x => Card.style.opacity = x, 0f, HideDuration)
                .SetEase(Ease.InQuad));

            _animationSequence.Join(DOTween.To(() => Card.style.translate.value.y.value,
                    y => Card.style.translate = new Translate(0, y, 0), 30f, HideDuration)
                .SetEase(Ease.InBack));

            _animationSequence.Join(DOTween.To(() => Card.style.scale.value.value.x,
                    s => Card.style.scale = new Scale(new Vector2(s, s)), 0.95f, HideDuration)
                .SetEase(Ease.InBack));

            await _animationSequence.Play().ToUniTask();

            Root.visible = false;
        }
    }
}