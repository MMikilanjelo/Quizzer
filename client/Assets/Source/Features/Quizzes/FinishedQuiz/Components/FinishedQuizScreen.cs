using Source.Features.Quizzes.FinishedQuiz.ViewModels;
using Source.Shared.Components.Elements.Label;
using Source.Shared.Components.Headers.FlowHeader;
using Source.Shared.Components.List;
using Source.Shared.Components.Screens;
using Source.Shared.Components.Skeleton;
using Source.Shared.Extensions;
using Source.Shared.Icons;
using Source.Shared.Reactive;
using Source.Shared.Reactive.Disposables;
using Source.Shared.Services;
using UnityEngine;
using UnityEngine.UIElements;

namespace Source.Features.Quizzes.FinishedQuiz.Components
{
    public class FinishedQuizScreen : IScreenView, IScreenWithHeaderView
    {
        public VisualElement Root { get; }
        public VisualElement Header { get; }

        private readonly IFinishedQuizScreenViewModel _viewModel;
        private readonly KnowledgeAreasCard _knowledgeAreasCard;
        private readonly PerformanceBreakdownCard _performanceBreakdownCard;
        private readonly PerformanceBreakdownSkeleton _performanceBreakdownSkeleton;
        private readonly ReactiveVisualElementList _knowledgeAreasSkeletonList;
        private readonly Skeleton _mainTitleSkeleton;
        private readonly FlowHeader _flowHeader;
        private readonly CustomLabel _quizNameLabel;
        private CompositeDisposable _disposable;

        public FinishedQuizScreen(TemplateContainer visualElement, IFinishedQuizScreenViewModel viewModel, IIconProvider iconProvider)
        {
            _viewModel = viewModel;

            _flowHeader = new FlowHeader(iconProvider.Get(Icons.ChevronLeft))
            {
                BackButton = { style = { visibility = Visibility.Visible } },
                Dots = { style = { visibility = Visibility.Hidden } }
            };

            Root = visualElement;

            var scrollContainer = new ScrollView(ScrollViewMode.Vertical)
            {
                name = "ScrollContainer"
            };

            scrollContainer.AddToClassList("screen__container");

            _quizNameLabel = new CustomLabel
            {
                Variant = CustomLabel.TextVariant.Title2,
                Weight = CustomLabel.FontWeight.Bold,
                text = string.Empty,
                style = { marginBottom = 16 }
            };

            _mainTitleSkeleton = new Skeleton
            {
                Variant = Skeleton.SkeletonVariant.Title1,
                style =
                {
                    width = Length.Percent(30),
                    marginBottom = 16
                }
            };

            _performanceBreakdownCard = new PerformanceBreakdownCard();
            _performanceBreakdownSkeleton = new PerformanceBreakdownSkeleton();

            _knowledgeAreasCard = new KnowledgeAreasCard(iconProvider);
            _knowledgeAreasSkeletonList = new ReactiveVisualElementList
            {
                Direction = FlexDirection.Column,
                ColumnGap = 8
            };

            scrollContainer.Add(_quizNameLabel);
            scrollContainer.Add(_mainTitleSkeleton);

            scrollContainer.Add(_performanceBreakdownCard);
            scrollContainer.Add(_performanceBreakdownSkeleton);

            scrollContainer.Add(_knowledgeAreasCard);
            scrollContainer.Add(_knowledgeAreasSkeletonList);

            Root.Add(scrollContainer);
            Header = _flowHeader;
        }

        public void Initialize()
        {
            _disposable = new CompositeDisposable();

            _knowledgeAreasSkeletonList.Add(new KnowledgeAreaSubCardSkeleton());
            _knowledgeAreasSkeletonList.Add(new KnowledgeAreaSubCardSkeleton());
            _knowledgeAreasSkeletonList.Add(new KnowledgeAreaSubCardSkeleton());

            _flowHeader.BackButton.Bind(_viewModel.GoBackCommand).AddTo(_disposable);
            _viewModel.QuizName.Subscribe(val => _quizNameLabel.text = val).AddTo(_disposable);

            _knowledgeAreasCard.Bind(_viewModel.KnowledgeAreas).AddTo(_disposable);
            _performanceBreakdownCard.Bind(_viewModel.Performance).AddTo(_disposable);

            _viewModel.IsLoading.Subscribe(UpdateViewState).AddTo(_disposable);
            UpdateViewState(_viewModel.IsLoading.Value);
        }

        public void Dispose()
        {
            _disposable.Dispose();
        }

        private void UpdateViewState(bool isLoading)
        {
            if (isLoading)
            {
                _mainTitleSkeleton.style.display = DisplayStyle.Flex;
                _performanceBreakdownSkeleton.style.display = DisplayStyle.Flex;
                _knowledgeAreasSkeletonList.style.display = DisplayStyle.Flex;

                _quizNameLabel.style.display = DisplayStyle.None;
                _performanceBreakdownCard.style.display = DisplayStyle.None;
                _knowledgeAreasCard.style.display = DisplayStyle.None;
            }
            else
            {
                _mainTitleSkeleton.style.display = DisplayStyle.None;
                _performanceBreakdownSkeleton.style.display = DisplayStyle.None;
                _knowledgeAreasSkeletonList.style.display = DisplayStyle.None;

                _quizNameLabel.style.display = DisplayStyle.Flex;
                _performanceBreakdownCard.style.display = DisplayStyle.Flex;
                _knowledgeAreasCard.style.display = DisplayStyle.Flex;
            }
        }
    }
}