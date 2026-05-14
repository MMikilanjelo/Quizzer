using System;
using Source.Features.TabBar.Models;
using Source.Features.TabBar.ViewModels;
using Source.Shared.Components.Manipulators;
using Source.Shared.Reactive.Commands;
using Source.Shared.Reactive.Events;
using Source.Shared.Services;
using UnityEngine;
using UnityEngine.UIElements;

namespace Source.Features.TabBar.Components
{
    public class TabItem : VisualElement
    {
        public IReadOnlyReactiveEvent<TabBarItemViewModel> ItemClicked => _itemClicked;

        private readonly ReactiveEvent<TabBarItemViewModel> _itemClicked = new();
        private readonly VisualElement _icon;
        private readonly IIconProvider _iconProvider;

        public TabItem(IIconProvider iconProvider)
        {
            _iconProvider = iconProvider;
            pickingMode = PickingMode.Position;

            AddToClassList("tab-item");
            _icon = new VisualElement();
            _icon.AddToClassList("tab-item__icon");
            _icon.pickingMode = PickingMode.Ignore;
            Add(_icon);
        }

        public void Bind(TabBarItemViewModel viewModel)
        {
            _icon.style.backgroundImage = new StyleBackground(_iconProvider.Get(viewModel.Model.Icon));

            this.AddManipulator(new Clickable(() => _itemClicked.Invoke(viewModel)));
        }

        public void SetSelection(bool isSelected)
        {
            if (isSelected)
            {
                AddToClassList("tab-item--active");
            }
            else
            {
                RemoveFromClassList("tab-item--active");
            }
        }
    }
}