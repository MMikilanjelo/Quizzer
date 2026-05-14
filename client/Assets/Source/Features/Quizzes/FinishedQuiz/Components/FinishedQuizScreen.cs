using Source.Features.Quizzes.FinishedQuiz.ViewModels;
using Source.Shared.Components.Elements.Label;
using Source.Shared.Components.Headers.FlowHeader;
using Source.Shared.Components.Screens;
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
                text = "Quiz #4",
                style = { marginBottom = 16 }
            };

            _knowledgeAreasCard = new KnowledgeAreasCard(iconProvider);
            _performanceBreakdownCard = new PerformanceBreakdownCard();

            scrollContainer.Add(_quizNameLabel);
            scrollContainer.Add(_performanceBreakdownCard);
            scrollContainer.Add(_knowledgeAreasCard);

            Root.Add(scrollContainer);
            Header = _flowHeader;
        }

        public void Initialize()
        {
            _disposable = new CompositeDisposable();

            _flowHeader.BackButton.Bind(_viewModel.GoBackCommand).AddTo(_disposable);
            _viewModel.QuizName.Subscribe(val => _quizNameLabel.text = val).AddTo(_disposable);

            _knowledgeAreasCard.Bind(_viewModel.KnowledgeAreas).AddTo(_disposable);
            _performanceBreakdownCard.Bind(_viewModel.Performance).AddTo(_disposable);
        }

        public void Dispose()
        {
            _disposable.Dispose();
        }
    }
}