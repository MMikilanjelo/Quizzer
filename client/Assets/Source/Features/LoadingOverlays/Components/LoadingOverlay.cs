using DG.Tweening;
using Source.Shared.Components.Elements.ProgressBar;
using Source.Shared.Components.Overlays;
using UnityEngine.UIElements;

namespace Source.Features.LoadingOverlays.Components
{
    internal class LoadingOverlay : IOverlayView
    {
        public VisualElement Root { get; }

        private readonly VisualElement[] _dots = new VisualElement[3];
        private readonly CustomProgressBar _progressBar;

        private Sequence _bounceSequence;

        public LoadingOverlay(VisualElement root)
        {
            Root = root;
            Root.style.flexGrow = 1;
            Root.style.width = new Length(100, LengthUnit.Percent);
            Root.style.height = new Length(100, LengthUnit.Percent);

            _dots[0] = Root.Q<VisualElement>("dot1");
            _dots[1] = Root.Q<VisualElement>("dot2");
            _dots[2] = Root.Q<VisualElement>("dot3");
            _progressBar = Root.Q<CustomProgressBar>();
        }

        public void Initialize()
        {
            CreateDotSequence();

            _bounceSequence.Play();
        }

        public void Dispose()
        {
            _bounceSequence?.Kill();
        }

        public void SetProgress(float progressValue) =>
            _progressBar.SetNormalizedValue(progressValue);

        private void CreateDotSequence()
        {
            _bounceSequence = DOTween.Sequence();

            for (int i = 0; i < _dots.Length; i++)
            {
                var dot = _dots[i];

                Tween moveUp = DOTween.To(() => 0f, y => { dot.style.translate = new Translate(0, new Length(y, LengthUnit.Pixel)); }, -15f, 0.3f).SetEase(Ease.OutQuad);

                Tween moveDown = DOTween.To(() => -15f, y => { dot.style.translate = new Translate(0, new Length(y, LengthUnit.Pixel)); }, 0f, 0.3f).SetEase(Ease.InQuad);

                _bounceSequence.Insert(i * 0.15f, moveUp);
                _bounceSequence.Insert(i * 0.15f + 0.3f, moveDown);
            }

            _bounceSequence.SetLoops(-1);
        }
    }
}