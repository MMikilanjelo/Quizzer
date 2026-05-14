using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Source.Features.TabBar.Mediator;
using Source.Features.TabBar.ViewModels;
using Source.Shared.Components;
using Source.Shared.Extensions;
using Source.Shared.Reactive;
using Source.Shared.Reactive.Disposables;
using Source.Shared.Services;
using UnityEngine.UIElements;

namespace Source.Features.TabBar.Components
{
    public class TabBar : VisualElement, IView
    {
        public VisualElement Root => this;

        private CompositeDisposable _disposable;

        private ITabBarMediator _mediator;
        private readonly IIconProvider _iconProvider;

        public TabBar(IIconProvider iconProvider)
        {
            _iconProvider = iconProvider;
            AddToClassList("tab-bar");
        }

        public void Bind(ITabBarMediator mediator)
        {
            _mediator = mediator;

            _disposable?.Dispose();
            _disposable = new CompositeDisposable();

            mediator.TabBarItems.Added.Subscribe(AddTab).AddTo(_disposable);
            mediator.IsTabBarVisible.Subscribe(ToggleVisibility).AddTo(_disposable);

            BindTabs(mediator.TabBarItems);
            ToggleVisibility(mediator.IsTabBarVisible.Value);
        }

        private void Show()
        {
            if (style.display.value == DisplayStyle.Flex)
            {
                return;
            }

            style.display = DisplayStyle.Flex;
        }

        private void Hide()
        {
            if (style.display.value == DisplayStyle.None)
            {
                return;
            }

            style.display = DisplayStyle.None;
        }

        private void BindTabs(IReadOnlyList<TabBarItemViewModel> viewModels)
        {
            Clear();

            foreach (var viewModel in viewModels)
            {
                AddTab(viewModel);
            }
        }

        private void AddTab(TabBarItemViewModel viewModel)
        {
            var tab = new TabItem(_iconProvider);

            tab.Bind(viewModel);

            tab.ItemClicked
                .Subscribe(vm => _mediator.SelectTabCommand.Execute(vm))
                .AddTo(_disposable);

            tab
                .BindProperty(
                    viewModel.IsSelected,
                    (t, isSelected) => t.SetSelection(isSelected)
                ).AddTo(_disposable);

            tab.SetSelection(viewModel.IsSelected.Value);

            Add(tab);
        }

        private void ToggleVisibility(bool isVisible)
        {
            if (isVisible)
            {
                Show();
            }
            else
            {
                Hide();
            }
        }
    }
}