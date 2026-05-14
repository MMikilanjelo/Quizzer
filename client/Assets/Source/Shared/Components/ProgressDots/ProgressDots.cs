using System;
using UnityEngine.UIElements;

namespace Source.Shared.Components.ProgressDots
{
    [UxmlElement]
    public partial class ProgressDots : VisualElement
    {
        private int _totalSteps = 1;
        private int _currentStep = 1;

        [UxmlAttribute]
        public int TotalSteps
        {
            get => _totalSteps;
            set
            {
                _totalSteps = Math.Max(1, value);

                if (_currentStep > _totalSteps)
                {
                    _currentStep = _totalSteps;
                }

                RebuildDots();
            }
        }

        [UxmlAttribute]
        public int CurrentStep
        {
            get => _currentStep;
            set
            {
                _currentStep = Math.Clamp(value, 1, _totalSteps);
                UpdateActiveDot();
            }
        }

        public ProgressDots()
        {
            AddToClassList("progress-dots-container");
            RebuildDots();
        }

        private void RebuildDots()
        {
            Clear();

            for (var i = 0; i < _totalSteps; i++)
            {
                var dot = new VisualElement();
                dot.AddToClassList("progress-dot");
                Add(dot);
            }

            UpdateActiveDot();
        }

        private void UpdateActiveDot()
        {
            if (childCount != _totalSteps)
            {
                return;
            }

            for (int i = 0; i < childCount; i++)
            {
                var dot = ElementAt(i);

                if (i + 1 == _currentStep)
                {
                    dot.AddToClassList("progress-dot--active");
                }
                else
                {
                    dot.RemoveFromClassList("progress-dot--active");
                }
            }
        }
    }
}