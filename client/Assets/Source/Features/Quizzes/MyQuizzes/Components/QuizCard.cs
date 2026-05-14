using System;
using Source.Features.Quizzes.MyQuizzes.ViewModels;
using Source.Shared.Components.Elements.Badge;
using Source.Shared.Components.Elements.Button;
using Source.Shared.Components.Elements.Label;
using Source.Shared.Components.Elements.ProgressBar;
using Source.Shared.Components.List;
using Source.Shared.Icons;
using Source.Shared.Reactive;
using Source.Shared.Reactive.Disposables;
using Source.Shared.Services;
using UnityEngine.UIElements;

namespace Source.Features.Quizzes.MyQuizzes.Components
{
    public class QuizCard : VisualElement
    {
        private readonly CustomLabel _titleLabel;
        private readonly Badge _statusBadge;
        private readonly CustomLabel _questionsLabel;
        private readonly CustomButton _actionButton;
        private readonly CustomProgressBar _progressBar;

        private readonly ReactiveVisualElementList<string> _quizTagsList;

        private CompositeDisposable _disposable;

        public QuizCard()
        {
            AddToClassList("quiz-card");

            var headerContainer = new VisualElement();
            headerContainer.AddToClassList("quiz-card__header");

            var titleRow = new VisualElement();
            titleRow.AddToClassList("quiz-card__title-row");

            _titleLabel = new CustomLabel
            {
                Variant = CustomLabel.TextVariant.Large,
                Weight = CustomLabel.FontWeight.SemiBold,
                text = "Title"
            };

            _statusBadge = new Badge
            {
                Size = Badge.BadgeSize.Small,
                Shape = Badge.BadgeShape.Rounded,
            };

            titleRow.Add(_titleLabel);
            titleRow.Add(_statusBadge);

            _quizTagsList = new ReactiveVisualElementList<string>
            {
                RowGap = 4,
                ColumnGap = 4,
                Direction = FlexDirection.Row,
                Wrap = Wrap.Wrap,
                style = { marginBottom = 4 }
            };
            _quizTagsList.AddToClassList("quiz-card__tags-row");

            headerContainer.Add(titleRow);
            headerContainer.Add(_quizTagsList);

            var statsContainer = new VisualElement();
            statsContainer.AddToClassList("quiz-card__stats");

            _questionsLabel = new CustomLabel
            {
                Variant = CustomLabel.TextVariant.Small,
                Weight = CustomLabel.FontWeight.Regular
            };
            _questionsLabel.AddToClassList("quiz-card__questions-label");

            statsContainer.Add(_questionsLabel);

            _progressBar = new CustomProgressBar();
            _progressBar.AddToClassList("quiz-card__progress-bar");

            _actionButton = new CustomButton
            {
                Shape = CustomButton.ButtonShape.Pill,
                Variant = CustomButton.ButtonVariant.Ghost,
                Text = "Continue",
            };

            Add(headerContainer);
            Add(statsContainer);
            Add(_progressBar);
            Add(_actionButton);
        }

        public IDisposable Bind(QuizItemViewModel viewModel)
        {
            _disposable?.Dispose();
            _disposable = new CompositeDisposable();

            viewModel.Name.Subscribe(value => _titleLabel.text = value);
            viewModel.Status.Subscribe(UpdateStatus);
            viewModel.ProgressCount.Subscribe(value => _questionsLabel.text = $"{value}/{viewModel.TotalCount.Value} Questions");
            viewModel.NormalizedProgress.Subscribe(value => _progressBar.SetNormalizedValue(value));
            _actionButton.Bind(viewModel.ActionCommand).AddTo(_disposable);

            _quizTagsList.Bind(
                makeItem: () => new Badge
                {
                    Size = Badge.BadgeSize.Small,
                    Variant = Badge.BadgeVariant.Neutral,
                    Shape = Badge.BadgeShape.Rounded
                },
                bindItem: (badge, tag, _) => badge.Text = tag
            );

            _quizTagsList.Set(viewModel.Topics);
            _titleLabel.text = viewModel.Name.Value;
            _questionsLabel.text = $"{viewModel.ProgressCount.Value}/{viewModel.TotalCount.Value} Questions";
            _progressBar.SetNormalizedValue(viewModel.NormalizedProgress.Value);

            UpdateStatus(viewModel.Status.Value);

            return _disposable;
        }

        private void UpdateStatus(string status)
        {
            switch (status.ToLower())
            {
                case "inprogress":
                case "ready":
                    _statusBadge.Variant = Badge.BadgeVariant.Info;
                    _statusBadge.Text = "Active";

                    _progressBar.style.display = DisplayStyle.Flex;
                    _progressBar.Variant = CustomProgressBar.ProgressBarVariant.Primary;

                    _actionButton.style.display = DisplayStyle.Flex;
                    _actionButton.Text = status.ToLower() == "inprogress" ? "Continue" : "Start";
                    _actionButton.Variant = CustomButton.ButtonVariant.Secondary;
                    break;

                case "completed":
                    _statusBadge.Variant = Badge.BadgeVariant.Success;
                    _statusBadge.Text = "Done";

                    _progressBar.style.display = DisplayStyle.Flex;
                    _progressBar.Variant = CustomProgressBar.ProgressBarVariant.Success;

                    _actionButton.style.display = DisplayStyle.Flex;
                    _actionButton.Text = "View Results";
                    _actionButton.Variant = CustomButton.ButtonVariant.Ghost;
                    break;

                default:
                    _statusBadge.style.visibility = Visibility.Hidden;
                    _progressBar.style.display = DisplayStyle.None;
                    _actionButton.style.display = DisplayStyle.None;
                    break;
            }
        }
    }
}