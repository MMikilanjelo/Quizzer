using System;
using System.Collections.Generic;
using Source.Features.Quizzes.FinishedQuiz.ViewModels;
using Source.Shared.Components.Elements.Label;
using Source.Shared.Components.List;
using Source.Shared.Reactive;
using Source.Shared.Reactive.Disposables;
using Source.Shared.Reactive.List;
using Source.Shared.Services;
using UnityEngine;
using UnityEngine.UIElements;

namespace Source.Features.Quizzes.FinishedQuiz.Components
{
    public class KnowledgeAreasCard : VisualElement
    {
        private readonly ReactiveVisualElementList<KnowledgeAreaItemViewModel> _areasList;
        private CompositeDisposable _disposable;
        private readonly IIconProvider _iconProvider;

        public KnowledgeAreasCard(IIconProvider iconProvider)
        {
            _iconProvider = iconProvider;
            AddToClassList("knowledge-card");

            var title = new CustomLabel
            {
                Variant = CustomLabel.TextVariant.Large,
                Weight = CustomLabel.FontWeight.SemiBold,
                text = "Knowledge Areas"
            };
            title.AddToClassList("knowledge-card__title");
            Add(title);

            _areasList = new ReactiveVisualElementList<KnowledgeAreaItemViewModel>
            {
                Direction = FlexDirection.Column,
                ColumnGap = 8
            };
            Add(_areasList);
        }

        public IDisposable Bind(IReadOnlyReactiveList<KnowledgeAreaItemViewModel> viewModels)
        {
            _disposable?.Dispose();
            _disposable = new CompositeDisposable();

            _areasList.Bind(
                makeItem: () => new KnowledgeAreaSubCard(_iconProvider),
                bindItem: (view, itemVm, disposable) => view
                    .Bind(itemVm)
                    .AddTo(disposable)
            );

            _areasList.Set(viewModels);

            return _disposable;
        }
    }

    public class KnowledgeAreaSubCard : VisualElement
    {
        private readonly CustomLabel _titleLabel;
        private readonly VisualElement _statusIcon;
        private readonly CustomLabel _statusLabel;
        private readonly CustomLabel _progressionLabel;
        private readonly IIconProvider _iconProvider;

        public KnowledgeAreaSubCard(IIconProvider iconProvider)
        {
            _iconProvider = iconProvider;

            AddToClassList("knowledge-sub-card");

            var leftCol = new VisualElement();
            leftCol.AddToClassList("knowledge-sub-card__left");

            _titleLabel = new CustomLabel
            {
                Variant = CustomLabel.TextVariant.Regular,
                Weight = CustomLabel.FontWeight.SemiBold,
                style = { marginBottom = 4 }
            };

            var statusRow = new VisualElement();
            statusRow.AddToClassList("knowledge-sub-card__status-row");

            _statusIcon = new VisualElement();
            _statusIcon.AddToClassList("knowledge-sub-card__status-icon");

            _statusLabel = new CustomLabel
            {
                Variant = CustomLabel.TextVariant.Small,
                Weight = CustomLabel.FontWeight.Regular
            };

            statusRow.Add(_statusIcon);
            statusRow.Add(_statusLabel);

            leftCol.Add(_titleLabel);
            leftCol.Add(statusRow); 

            _progressionLabel = new CustomLabel
            {
                Variant = CustomLabel.TextVariant.Tiny,
                Weight = CustomLabel.FontWeight.Regular
            };
            _progressionLabel.AddToClassList("knowledge-sub-card__progression");

            Add(leftCol);
            Add(_progressionLabel);
        }

        public IDisposable Bind(KnowledgeAreaItemViewModel viewModel)
        {
            _titleLabel.text = viewModel.TopicName;
            _progressionLabel.text = $"{viewModel.StartPercentage}% → {viewModel.EndPercentage}%";
            _statusLabel.text = viewModel.StatusText;

            var icon = _iconProvider.Get(viewModel.Icon);

            if (viewModel.NeedsReview)
            {
                EnableInClassList("knowledge-sub-card--warning", true);
                _statusLabel.RemoveFromClassList("text-success");
                _statusLabel.AddToClassList("text-warning");

                _statusIcon.style.backgroundImage = new StyleBackground(icon);
                _statusIcon.AddToClassList("icon-warning");
                _statusIcon.RemoveFromClassList("icon-success");
            }
            else
            {
                EnableInClassList("knowledge-sub-card--warning", false);
                _statusLabel.RemoveFromClassList("text-warning");
                _statusLabel.AddToClassList("text-success");

                _statusIcon.style.backgroundImage = new StyleBackground(icon);
                _statusIcon.AddToClassList("icon-success");
                _statusIcon.RemoveFromClassList("icon-warning");
            }

            return Disposable.Empty;
        }
    }
}