using Source.Features.Quizzes.CreateQuiz.Models;
using Source.Features.Quizzes.CreateQuiz.ViewModels;
using Source.Shared.Components.Elements.Button;
using Source.Shared.Components.Elements.Chip;
using Source.Shared.Components.Elements.Label;
using Source.Shared.Components.Headers.FlowHeader;
using Source.Shared.Components.Layouts.ActionFooter;
using Source.Shared.Components.List;
using Source.Shared.Components.Screens;
using Source.Shared.Icons;
using Source.Shared.Reactive;
using Source.Shared.Reactive.Disposables;
using Source.Shared.Services;
using UnityEngine.UIElements;

namespace Source.Features.Quizzes.CreateQuiz.Components
{
    public class CreateQuizScreen : IScreenView, IScreenWithHeaderView, IScreenWithFooterView
    {
        public VisualElement Root { get; }
        public VisualElement Header { get; }
        public VisualElement Footer { get; }

        private readonly ReactiveVisualElementList<SelectDomainItemViewModel> _domainList;
        private readonly ReactiveVisualElementList<SelectDifficultyLevelItemViewModel> _difficultyLevelsList;

        private readonly CustomButton _createButton;
        private readonly FlowHeader _flowHeader;
        private readonly ModeSelectionTile _smartSelectModeTile;
        private readonly ModeSelectionTile _manualSelectModeTile;
        private readonly SelectNumberOfQuestionView _selectNumberOfQuestionView;
        private readonly VisualElement _smartInfoSurface;
        private readonly VisualElement _manualInfoSurface;

        private CompositeDisposable _disposable;
        private readonly ICreateQuizScreenViewModel _viewModel;


        public CreateQuizScreen(
            TemplateContainer visualElement,
            ICreateQuizScreenViewModel viewModel,
            IIconProvider iconProvider
        )
        {
            _viewModel = viewModel;

            Root = visualElement;
            Root.AddToClassList("screen");

            var screenContainer = new ScrollView(ScrollViewMode.Vertical);
            screenContainer.AddToClassList("screen__container");

            var title = new CustomLabel
            {
                Variant = CustomLabel.TextVariant.Title2,
                Weight = CustomLabel.FontWeight.Bold,
                text = "Create Quiz",
                style = { marginBottom = 24 }
            };

            var configurationModeGroup = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    justifyContent = Justify.SpaceBetween,
                    marginBottom = 24
                }
            };

            _smartSelectModeTile = new ModeSelectionTile("Smart select", "We picks for you", iconProvider.Get(Icons.Sparkle));
            _manualSelectModeTile = new ModeSelectionTile("Manual", "Chose yourself", iconProvider.Get(Icons.SlidersHorizontal))
            {
                style = { marginLeft = 16 }
            };

            configurationModeGroup.Add(_smartSelectModeTile);
            configurationModeGroup.Add(_manualSelectModeTile);

            _smartInfoSurface = new VisualElement();
            var smartInfo = new SmartInfoCard(iconProvider.Get(Icons.Sparkle));
            _smartInfoSurface.Add(smartInfo);

            _manualInfoSurface = new VisualElement();

            var domainContainer = new VisualElement
            {
                style = { marginBottom = 24 }
            };

            var selectDomainLabel = new CustomLabel
            {
                Variant = CustomLabel.TextVariant.Small,
                Weight = CustomLabel.FontWeight.Regular,
                text = "Select Domains",
                style = { marginBottom = 8 }
            };

            _domainList = new ReactiveVisualElementList<SelectDomainItemViewModel>
            {
                RowGap = 8,
                ColumnGap = 8,
                Direction = FlexDirection.Row,
                Wrap = Wrap.Wrap
            };

            domainContainer.Add(selectDomainLabel);
            domainContainer.Add(_domainList);

            var selectDifficultyLevelContainer = new VisualElement
            {
                style = { marginBottom = 24 }
            };

            var selectDifficultyLevelLabel = new CustomLabel
            {
                Variant = CustomLabel.TextVariant.Small,
                Weight = CustomLabel.FontWeight.Regular,
                text = "Select difficulty level",
                style = { marginBottom = 8 }
            };

            _difficultyLevelsList = new ReactiveVisualElementList<SelectDifficultyLevelItemViewModel>
            {
                RowGap = 8,
                ColumnGap = 8,
                Direction = FlexDirection.Row,
                Wrap = Wrap.NoWrap
            };

            selectDifficultyLevelContainer.Add(selectDifficultyLevelLabel);
            selectDifficultyLevelContainer.Add(_difficultyLevelsList);

            _selectNumberOfQuestionView = new SelectNumberOfQuestionView(iconProvider);

            _manualInfoSurface.Add(domainContainer);
            _manualInfoSurface.Add(selectDifficultyLevelContainer);
            _manualInfoSurface.Add(_selectNumberOfQuestionView);

            _createButton = new CustomButton
            {
                Text = "Create",
                Variant = CustomButton.ButtonVariant.Primary
            };

            var footer = new ActionFooter();
            footer.AddAction(_createButton);
            footer.RegisterCallback<GeometryChangedEvent>(geoEvt =>
            {
                var h = geoEvt.newRect.height;

                screenContainer.style.paddingBottom = h + 24f;
            });

            _flowHeader = new FlowHeader(iconProvider.Get(Icons.ChevronLeft))
            {
                BackButton = { style = { visibility = Visibility.Visible } },
                Dots = { style = { visibility = Visibility.Hidden } }
            };

            screenContainer.Add(title);
            screenContainer.Add(configurationModeGroup);
            screenContainer.Add(_smartInfoSurface);
            screenContainer.Add(_manualInfoSurface);
            Root.Add(screenContainer);

            Footer = footer;
            Header = _flowHeader;
        }

        public void Initialize()
        {
            _disposable = new CompositeDisposable();

            _flowHeader.BackButton.Bind(_viewModel.GoBackCommand).AddTo(_disposable);
            _createButton.Bind(_viewModel.CreateQuizCommand).AddTo(_disposable);

            _domainList.Bind(
                makeItem: () => new Chip(),
                bindItem: (chip, model, disposable) =>
                {
                    chip.Text = model.Name;
                    chip.Variant = model.IsSelected.Value ? Chip.ChipVariant.Filled : Chip.ChipVariant.Outline;
                    chip.BindProperty(model.IsSelected, (c, isSelected) => { c.Variant = isSelected ? Chip.ChipVariant.Filled : Chip.ChipVariant.Outline; })
                        .AddTo(disposable);
                }
            );

            _domainList.Set(_viewModel.Domains);

            _domainList.ItemClicked
                .Subscribe(vm => _viewModel.SelectDomainCommand.Execute(vm))
                .AddTo(_disposable);

            _difficultyLevelsList.Bind(
                makeItem: () => new Chip(),
                bindItem: (chip, model, disposable) =>
                {
                    chip.Text = model.Name;
                    chip.Variant = model.IsSelected.Value ? Chip.ChipVariant.Filled : Chip.ChipVariant.Outline;
                    chip
                        .BindProperty(model.IsSelected, (c, isSelected) => { c.Variant = isSelected ? Chip.ChipVariant.Filled : Chip.ChipVariant.Outline; })
                        .AddTo(disposable);
                }
            );

            _difficultyLevelsList.Set(_viewModel.DifficultyLevels);

            _difficultyLevelsList.ItemClicked
                .Subscribe(vm => _viewModel.SelectDifficultyLevelCommand.Execute(vm))
                .AddTo(_disposable);

            _selectNumberOfQuestionView
                .Bind(_viewModel.NumberOfQuestion)
                .AddTo(_disposable);

            _viewModel.CurrentMode
                .Subscribe(OnCurrentModeChanged)
                .AddTo(_disposable);

            _smartSelectModeTile
                .RegisterDisposableCallback<PointerDownEvent>(_ => _viewModel.ChangeModeCommand.Execute(QuizCreationMode.Smart))
                .AddTo(_disposable);

            _manualSelectModeTile
                .RegisterDisposableCallback<PointerDownEvent>(_ => _viewModel.ChangeModeCommand.Execute(QuizCreationMode.Manual))
                .AddTo(_disposable);

            OnCurrentModeChanged(_viewModel.CurrentMode.Value);
        }

        private void OnCurrentModeChanged(QuizCreationMode mode)
        {
            var isSmart = mode == QuizCreationMode.Smart;

            _smartSelectModeTile.IsSelected = isSmart;
            _manualSelectModeTile.IsSelected = !isSmart;

            _smartInfoSurface.style.display = isSmart ? DisplayStyle.Flex : DisplayStyle.None;
            _manualInfoSurface.style.display = isSmart ? DisplayStyle.None : DisplayStyle.Flex;
        }

        public void Dispose()
        {
            _disposable.Dispose();
        }
    }
}