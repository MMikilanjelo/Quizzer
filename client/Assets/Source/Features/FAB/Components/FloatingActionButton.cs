using System.Collections.Generic;
using Source.Features.FAB.Mediator;
using Source.Features.FAB.ViewModels;
using Source.Shared.Components;
using Source.Shared.Components.Elements.Button;
using Source.Shared.Icons;
using Source.Shared.Reactive;
using Source.Shared.Reactive.Disposables;
using Source.Shared.Services;
using UnityEngine.UIElements;

namespace Source.Features.FAB.Components
{
    public class FloatingActionButton : VisualElement, IView
    {
        public VisualElement Root => this;

        private CompositeDisposable _mediatorBindings;
        private CompositeDisposable _actionsBindings;

        private readonly VisualElement _menu;
        private readonly VisualElement _fabIcon;
        private readonly CustomButton _fabButton;
        private readonly IIconProvider _iconProvider;
        private IFabMediator _fabMediator;

        public FloatingActionButton(IIconProvider iconProvider)
        {
            pickingMode = PickingMode.Ignore;
            _iconProvider = iconProvider;

            AddToClassList("fab-container");

            _menu = new VisualElement
            {
                name = "FabMenu",
                pickingMode = PickingMode.Ignore
            };
            _menu.AddToClassList("fab__menu");

            _fabButton = new CustomButton
            {
                name = "FabButton",
                Text = string.Empty,
                Variant = CustomButton.ButtonVariant.Primary,
                Shape = CustomButton.ButtonShape.Circle,
                Size = CustomButton.ButtonSize.Large
            };

            _fabIcon = new VisualElement
            {
                name = "FabIcon",
                pickingMode = PickingMode.Ignore
            };
            _fabIcon.AddToClassList("fab__icon");
            _fabIcon.style.backgroundImage = new StyleBackground(_iconProvider.Get(Icons.Plus));

            _fabButton.Add(_fabIcon);
            Add(_menu);
            Add(_fabButton);
        }

        public void Bind(IFabMediator mediator)
        {
            _fabMediator = mediator;

            _mediatorBindings?.Dispose();
            _mediatorBindings = new CompositeDisposable();

            _mediatorBindings.Add(mediator.FabMenuActions.Added.Subscribe(_ => BindActions(mediator.FabMenuActions)));
            BindActions(mediator.FabMenuActions);

            _fabMediator.IsFabOpen
                .Subscribe(SetOpenState)
                .AddTo(_mediatorBindings);

            _fabMediator.IsFabVisible
                .Subscribe(ToggleVisibility)
                .AddTo(_mediatorBindings);

            _fabButton
                .Bind(_fabMediator.ToggleFabCommand)
                .AddTo(_mediatorBindings);

            ToggleVisibility(_fabMediator.IsFabVisible.Value);
        }

        private void Show()
        {
            if (style.display.value == DisplayStyle.Flex) return;
            style.display = DisplayStyle.Flex;
        }

        private void Hide()
        {
            if (style.display.value == DisplayStyle.None) return;
            style.display = DisplayStyle.None;
        }

        private void ToggleVisibility(bool isVisible)
        {
            if (isVisible) Show();
            else Hide();
        }

        public void Dispose()
        {
            UnregisterOutsideClickListener();
            _mediatorBindings?.Dispose();
            _actionsBindings?.Dispose();
        }

        private void BindActions(IReadOnlyList<FabActionViewModel> viewModels)
        {
            _actionsBindings?.Dispose();
            _actionsBindings = new CompositeDisposable();

            _menu.Clear();

            for (var i = 0; i < viewModels.Count; i++)
            {
                var viewModel = viewModels[i];

                var delayIndex = viewModels.Count - 1 - i;

                AddAction(viewModel, delayIndex);

                viewModel.ExecuteCommand.Executed
                    .Subscribe(_ => _fabMediator.CloseFabCommand.Execute())
                    .AddTo(_actionsBindings);
            }
        }

        private void AddAction(FabActionViewModel actionViewModel, int delayIndex)
        {
            var wrapper = new VisualElement();
            wrapper.AddToClassList("fab__menu-item");

            var delaySeconds = delayIndex * 0.05f;
            wrapper.style.transitionDelay = new StyleList<TimeValue>(new List<TimeValue> { new(delaySeconds, TimeUnit.Second) });

            var actionButton = new FabMenuButton
            {
                Text = actionViewModel.Title,
                Icon = _iconProvider.Get(actionViewModel.Icon),
            };

            actionButton.Bind(actionViewModel.ExecuteCommand);
            wrapper.Add(actionButton);
            _menu.Add(wrapper);
        }

        private void SetOpenState(bool isOpen)
        {
            if (isOpen)
            {
                _menu.AddToClassList("fab__menu--active");
                _fabIcon.AddToClassList("fab__icon--active");
                RegisterOutsideClickListener();
            }
            else
            {
                _menu.RemoveFromClassList("fab__menu--active");
                _fabIcon.RemoveFromClassList("fab__icon--active");
                UnregisterOutsideClickListener();
            }
        }

        private void RegisterOutsideClickListener() =>
            panel?.visualTree?.RegisterCallback<PointerDownEvent>(OnPointerDownOutside, TrickleDown.TrickleDown);

        private void UnregisterOutsideClickListener() =>
            panel?.visualTree?.UnregisterCallback<PointerDownEvent>(OnPointerDownOutside, TrickleDown.TrickleDown);

        private void OnPointerDownOutside(PointerDownEvent evt)
        {
            var targetElement = evt.target as VisualElement;
            var clickedInsideFab = targetElement == this || Contains(targetElement);

            if (!clickedInsideFab)
            {
                _fabMediator?.CloseFabCommand?.Execute();
            }
        }
    }
}