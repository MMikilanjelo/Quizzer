using System;
using Source.Features.Quizzes.FinishedQuiz.ViewModels;
using Source.Shared.Components.Elements.Label;
using Source.Shared.Components.Elements.ProgressBar;
using Source.Shared.Components.Skeleton;
using Source.Shared.Reactive;
using Source.Shared.Reactive.Disposables;
using Source.Shared.Reactive.Events;
using UnityEngine.UIElements;

namespace Source.Features.Quizzes.FinishedQuiz.Components
{
    public class PerformanceBreakdownSkeleton : VisualElement
    {
        public PerformanceBreakdownSkeleton()
        {
            AddToClassList("performance-card");
            style.marginBottom = 16; 

            var titleSkeleton = new Skeleton { Variant = Skeleton.SkeletonVariant.Title2 };
            titleSkeleton.AddToClassList("performance-card__title");
            titleSkeleton.style.width = Length.Percent(60);
            titleSkeleton.style.marginBottom = 16;
            Add(titleSkeleton);

            Add(CreateRowSkeleton());
            Add(CreateRowSkeleton());
        }

        private VisualElement CreateRowSkeleton()
        {
            var container = new VisualElement();
            container.AddToClassList("performance-card__row");

            var textRow = new VisualElement();
            textRow.AddToClassList("performance-card__text-row");

            var labelSkeleton = new Skeleton
            {
                Variant = Skeleton.SkeletonVariant.Small,
                style =
                {
                    width = Length.Percent(40)
                }
            };

            textRow.Add(labelSkeleton);

            var progressBarSkeleton = new Skeleton { Variant = Skeleton.SkeletonVariant.Block };
            progressBarSkeleton.AddToClassList("performance-card__progress");
            progressBarSkeleton.style.height = 8; 
            progressBarSkeleton.style.marginTop = 8;

            container.Add(textRow);
            container.Add(progressBarSkeleton);

            return container;
        }
    }

    public class PerformanceBreakdownCard : VisualElement
    {
        private readonly CustomLabel _correctCountLabel;
        private readonly CustomProgressBar _correctProgressBar;

        private readonly CustomLabel _incorrectCountLabel;
        private readonly CustomProgressBar _incorrectProgressBar;

        private CompositeDisposable _disposable;

        public PerformanceBreakdownCard()
        {
            AddToClassList("performance-card");

            var title = new CustomLabel
            {
                Variant = CustomLabel.TextVariant.Large,
                Weight = CustomLabel.FontWeight.SemiBold,
                text = "Performance Breakdown"
            };
            title.AddToClassList("performance-card__title");
            Add(title);

            Add(CreateProgressRow("Correct Answers", out _correctCountLabel, out _correctProgressBar, CustomProgressBar.ProgressBarVariant.Success));

            Add(CreateProgressRow("Incorrect Answers", out _incorrectCountLabel, out _incorrectProgressBar, CustomProgressBar.ProgressBarVariant.Danger));
        }

        private VisualElement CreateProgressRow(string labelText, out CustomLabel countLabel, out CustomProgressBar progressBar, CustomProgressBar.ProgressBarVariant variant)
        {
            var container = new VisualElement();
            container.AddToClassList("performance-card__row");

            var textRow = new VisualElement();
            textRow.AddToClassList("performance-card__text-row");

            var label = new CustomLabel { Variant = CustomLabel.TextVariant.Large, text = labelText, Weight = CustomLabel.FontWeight.Regular };
            label.AddToClassList("performance-card__label");

            countLabel = new CustomLabel { Variant = CustomLabel.TextVariant.Large, Weight = CustomLabel.FontWeight.SemiBold };
            countLabel.AddToClassList("performance-card__count");

            textRow.Add(label);
            textRow.Add(countLabel);

            progressBar = new CustomProgressBar { Variant = variant };
            progressBar.AddToClassList("performance-card__progress");

            container.Add(textRow);
            container.Add(progressBar);

            return container;
        }

        public IDisposable Bind(PerformanceViewModel viewModel)
        {
            _disposable?.Dispose();
            _disposable = new CompositeDisposable();

            viewModel.CorrectNormalized.Subscribe(v => _correctProgressBar.SetNormalizedValue(v)).AddTo(_disposable);
            viewModel.IncorrectNormalized.Subscribe(v => _incorrectProgressBar.SetNormalizedValue(v)).AddTo(_disposable);

            _correctProgressBar.SetNormalizedValue(viewModel.CorrectNormalized.Value);
            _incorrectProgressBar.SetNormalizedValue(viewModel.IncorrectNormalized.Value);

            return _disposable;
        }
    }
}