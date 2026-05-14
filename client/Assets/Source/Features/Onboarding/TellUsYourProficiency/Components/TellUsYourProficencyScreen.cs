using Source.Features.Onboarding.TellUsYourProficiency.ViewModels;
using Source.Shared.Components.Elements.Button;
using Source.Shared.Components.Elements.Chip;
using Source.Shared.Components.Headers.FlowHeader;
using Source.Shared.Components.Layouts.ActionFooter;
using Source.Shared.Components.List;
using Source.Shared.Components.Screens;
using Source.Shared.Icons;
using Source.Shared.Reactive;
using Source.Shared.Reactive.Disposables;
using Source.Shared.Services;
using UnityEngine.UIElements;

namespace Source.Features.Onboarding.TellUsYourProficiency.Components
{
    internal class TellUsYourProficiencyScreen : IScreenView, IScreenWithFooterView, IScreenWithHeaderView
    {
        public VisualElement Root { get; }
        public VisualElement Footer { get; }
        public VisualElement Header { get; }

        private readonly ReactiveVisualElementList<ProficiencyItemViewModel> _interestsList;
        private readonly CustomButton _continueButton;
        private readonly ITellUsYourProficiencyScreenViewModel _viewModel;

        private CompositeDisposable _disposable;

        public TellUsYourProficiencyScreen(
            TemplateContainer visualElement,
            ITellUsYourProficiencyScreenViewModel viewModel,
            IIconProvider iconProvider
        )
        {
            _viewModel = viewModel;

            Root = visualElement;
            Root.AddToClassList("screen");

            var innerContainer = new VisualElement { name = "Container" };
            innerContainer.AddToClassList("screen__container");

            Root.Add(innerContainer);

            _interestsList = new ReactiveVisualElementList<ProficiencyItemViewModel>
            {
                ColumnGap = 6
            };

            var footer = new ActionFooter();
            _continueButton = new CustomButton
            {
                Text = "Continue",
                Variant = CustomButton.ButtonVariant.Primary
            };
            footer.AddAction(_continueButton);

            var flowHeader = new FlowHeader(iconProvider.Get(Icons.ChevronLeft))
            {
                BackButton = { style = { visibility = Visibility.Visible } },
                Dots =
                {
                    TotalSteps = _viewModel.TotalSteps,
                    CurrentStep = _viewModel.CurrentStep
                }
            };

            flowHeader.BackButton.Bind(_viewModel.GoBackCommand);

            Footer = footer;
            Header = flowHeader;

            innerContainer.Add(_interestsList);
        }

        public void Initialize()
        {
            _disposable = new CompositeDisposable();

            _continueButton
                .Bind(_viewModel.ContinueCommand)
                .AddTo(_disposable);

            _interestsList.Bind(
                makeItem: () => new Chip(),
                bindItem: (chip, model, disposable) =>
                {
                    chip.Text = model.Name;
                    chip.BindProperty(
                            model.IsSelected,
                            (c, isSelected) => c.Variant = isSelected
                                ? Chip.ChipVariant.Filled
                                : Chip.ChipVariant.Outline
                        )
                        .AddTo(disposable);
                }
            );

            _interestsList.Set(_viewModel.Proficiencies);

            _interestsList.ItemClicked
                .Subscribe(viewModel => _viewModel.SelectProficiencyCommand.Execute(viewModel))
                .AddTo(_disposable);
        }

        public void Dispose() =>
            _disposable?.Dispose();
    }
}