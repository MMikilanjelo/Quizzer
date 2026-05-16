using Cysharp.Threading.Tasks;
using Source.Features.Quizzes.ActiveQuiz.VIewModels;
using Source.Shared.Components.Elements.Badge;
using Source.Shared.Components.Elements.Button;
using Source.Shared.Components.Elements.Label;
using Source.Shared.Components.Elements.ProgressBar;
using Source.Shared.Components.Headers.FlowHeader;
using Source.Shared.Components.Layouts.ActionFooter;
using Source.Shared.Components.List;
using Source.Shared.Components.Screens;
using Source.Shared.Extensions;
using Source.Shared.Icons;
using Source.Shared.Reactive;
using Source.Shared.Reactive.Disposables;
using Source.Shared.Services;
using UnityEngine.UIElements;

namespace Source.Features.Quizzes.ActiveQuiz.Components
{
    public class ActiveQuizScreen :
        IScreenView,
        IScreenWithHeaderView,
        IScreenWithFooterView
    {
        public VisualElement Root { get; }
        public VisualElement Header { get; }
        public VisualElement Footer { get; }

        private readonly QuizQuestion _quizQuestion;
        private readonly ReactiveVisualElementList<QuizOptionViewModel> _optionsList;
        private readonly ReactiveVisualElementList<string> _quizTagsList;
        private readonly FlowHeader _flowHeader;
        private readonly CustomButton _continueButton;
        private readonly CustomLabel _quizNameLabel;
        private readonly CustomProgressBar _progressBar;

        private CompositeDisposable _disposable;

        private readonly IActiveQuizScreenViewModel _viewModel;

        public ActiveQuizScreen(
            TemplateContainer visualElement,
            IActiveQuizScreenViewModel viewModel,
            IIconProvider iconProvider
        )
        {
            _viewModel = viewModel;

            _flowHeader = new FlowHeader(iconProvider.Get(Icons.ChevronLeft))
            {
                BackButton = { style = { visibility = Visibility.Visible } },
                Dots = { style = { visibility = Visibility.Hidden } }
            };

            _quizNameLabel = new CustomLabel
            {
                Variant = CustomLabel.TextVariant.Title2,
                Weight = CustomLabel.FontWeight.Bold,
                text = "Quiz #4",
                style = { marginBottom = 16 }
            };

            Root = visualElement;
            Root.AddToClassList("screen");

            var screenContainer = new VisualElement { name = "Container" };
            screenContainer.AddToClassList("screen__container");

            _quizQuestion = new QuizQuestion
            {
                style = { marginBottom = 24 }
            };

            _optionsList = new ReactiveVisualElementList<QuizOptionViewModel>
            {
                Direction = FlexDirection.Column,
                ColumnGap = 8
            };

            var footer = new ActionFooter();
            _continueButton = new CustomButton
            {
                Text = "Continue",
                Variant = CustomButton.ButtonVariant.Primary
            };

            footer.AddAction(_continueButton);

            footer.RegisterCallback<GeometryChangedEvent>(geoEvt =>
            {
                var h = geoEvt.newRect.height;

                _optionsList.BottomPadding = h + 24f;
            });

            _progressBar = new CustomProgressBar
            {
                Variant = CustomProgressBar.ProgressBarVariant.Primary,
                style = { marginBottom = 24 }
            };

            _quizTagsList = new ReactiveVisualElementList<string>
            {
                RowGap = 4,
                ColumnGap = 4,
                Direction = FlexDirection.Row,
                Wrap = Wrap.NoWrap,
                style = { marginBottom = 24, flexGrow = 0 }
            };

            screenContainer.Add(_quizNameLabel);
            screenContainer.Add(_progressBar);
            screenContainer.Add(_quizTagsList);
            screenContainer.Add(_quizQuestion);
            screenContainer.Add(_optionsList);

            Root.Add(screenContainer);
            Header = _flowHeader;
            Footer = footer;
        }

        public void Initialize()
        {
            _disposable?.Dispose();
            _disposable = new CompositeDisposable();

            _flowHeader.BackButton.Bind(_viewModel.GoBackCommand).AddTo(_disposable);
            _continueButton.Bind(_viewModel.ContinueCommand).AddTo(_disposable);
            _quizQuestion.Bind(_viewModel.QuestionText).AddTo(_disposable);
            _viewModel.Progress.Subscribe(val => _progressBar.SetNormalizedValue(val)).AddTo(_disposable);
            _viewModel.QuizName.Subscribe(val => _quizNameLabel.text = val).AddTo(_disposable);

            _optionsList.Bind(
                makeItem: () => new QuizOption(),
                bindItem: (element, optionVm) => { element.Bind(optionVm).AddTo(element.Disposables); }
            );

            _optionsList.Set(_viewModel.Options);

            _optionsList.ItemClicked
                .Subscribe(vm => _viewModel.SelectOptionCommand.Execute(vm))
                .AddTo(_disposable);

            _quizTagsList.Bind(
                makeItem: () => new Badge
                {
                    Size = Badge.BadgeSize.Small,
                    Variant = Badge.BadgeVariant.Neutral,
                    Shape = Badge.BadgeShape.Rounded
                },
                bindItem: (badge, tag) => badge.Text = tag
            );

            _quizTagsList.Set(_viewModel.Topics);

            _progressBar.SetNormalizedValue(_viewModel.Progress.Value);

            _quizNameLabel.text = _viewModel.QuizName.Value;
        }

        public void Dispose()
        {
            _disposable.Dispose();
        }
    }
}