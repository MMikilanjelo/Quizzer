using Source.Features.Onboarding.TellUsYourGoal.ViewModels;
using Source.Shared.Components.Elements.Button;
using Source.Shared.Components.Elements.Chip;
using Source.Shared.Components.Headers.FlowHeader;
using Source.Shared.Components.Layouts.ActionFooter;
using Source.Shared.Components.Layouts.SafeArea;
using Source.Shared.Components.List;
using Source.Shared.Components.Screens;
using Source.Shared.Icons;
using Source.Shared.Reactive;
using Source.Shared.Reactive.Commands;
using Source.Shared.Reactive.Disposables;
using Source.Shared.Services;
using UnityEngine.UIElements;

namespace Source.Features.Onboarding.TellUsYourGoal.Components
{
    internal class TellUsYourGoalScreen : IScreenView, IScreenWithHeaderView, IScreenWithFooterView
    {
        public VisualElement Root { get; }
        public VisualElement Header { get; }
        public VisualElement Footer { get; }

        private readonly ReactiveVisualElementList<YourGoalItemViewModel> _goalsList;
        private readonly CustomButton _continueButton;

        private CompositeDisposable _disposable;

        private readonly ITellUsYourGoalScreenViewModel _viewModel;

        public TellUsYourGoalScreen(
            TemplateContainer visualElement,
            ITellUsYourGoalScreenViewModel viewModel,
            IIconProvider iconProvider
        )
        {
            Root = visualElement;
            Root.AddToClassList("screen");

            var innerContainer = new VisualElement { name = "Container" };
            innerContainer.AddToClassList("screen__container");

            Root.Add(innerContainer);

            _viewModel = viewModel;
            _goalsList = new ReactiveVisualElementList<YourGoalItemViewModel>
            {
                ColumnGap = 6,
            };

            var footer = new ActionFooter();
            _continueButton = new CustomButton
            {
                Text = "Continue",
                Variant = CustomButton.ButtonVariant.Primary
            };

            var header = new FlowHeader(iconProvider.Get(Icons.ChevronLeft))
            {
                BackButton = { style = { visibility = Visibility.Hidden } },
                Dots =
                {
                    TotalSteps = _viewModel.TotalSteps,
                    CurrentStep = _viewModel.CurrentStep
                }
            };

            Header = header;
            Footer = footer;

            innerContainer.Add(_goalsList);

            footer.AddAction(_continueButton);

            footer.RegisterCallback<GeometryChangedEvent>(geoEvt =>
            {
                float h = geoEvt.newRect.height;

                _goalsList.BottomPadding = h + 24f;
            });
        }

        public void Initialize()
        {
            _disposable = new CompositeDisposable();

            _continueButton
                .Bind(_viewModel.ContinueCommand)
                .AddTo(_disposable);

            _goalsList.Bind(
                makeItem: () => new Chip(),
                bindItem: (chip, model, disposable) =>
                {
                    chip.Text = model.Name;
                    chip.Variant = model.IsSelected.Value ? Chip.ChipVariant.Filled : Chip.ChipVariant.Outline;
                    chip.BindProperty(model.IsSelected, (c, isSelected) => { c.Variant = isSelected ? Chip.ChipVariant.Filled : Chip.ChipVariant.Outline; })
                        .AddTo(disposable);
                }
            );

            _goalsList.Set(_viewModel.Goals);

            _goalsList.ItemClicked
                .Subscribe(vm => _viewModel.SelectGoalCommand.Execute(vm))
                .AddTo(_disposable);
        }

        public void Dispose()
        {
            _disposable.Dispose();
        }
    }
}