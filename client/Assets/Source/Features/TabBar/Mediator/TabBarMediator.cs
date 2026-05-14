using System.Collections.Generic;
using Source.Features.TabBar.ViewModels;
using Source.Shared.Reactive.Commands;
using Source.Shared.Reactive.Events;
using Source.Shared.Reactive.List;
using Source.Shared.Reactive.SelectableList;
using Source.Shared.Services;
using Source.Shared.UIStack.Mediator;

namespace Source.Features.TabBar.Mediator
{
    public class TabBarMediator : ITabBarMediator
    {
        public IReadOnlyReactiveList<TabBarItemViewModel> TabBarItems => _tabBarItems.Items;
        public IReadOnlyReactiveProperty<bool> IsTabBarVisible => _isTabBarVisible;
        public IReadOnlyReactiveEvent<TabBarItemViewModel> TabSelectionChanged => _tabBarItems.ItemSelectionChanged;
        public ICommand<TabBarItemViewModel> SelectTabCommand => _tabBarItems.SelectCommand;

        private readonly SelectableList<TabBarItemViewModel> _tabBarItems = SelectableList<TabBarItemViewModel>.Exclusive();
        private readonly ReactiveProperty<bool> _isTabBarVisible = new(false);
        private readonly IFooterStackMediator _footerStackMediator;
        private readonly IAssetProviderService _assetProviderService;
        private Components.TabBar _tabBar;

        public TabBarMediator(
            IFooterStackMediator footerStackMediator,
            IAssetProviderService assetProviderService
        )
        {
            _footerStackMediator = footerStackMediator;
            _assetProviderService = assetProviderService;
        }

        public void Initialize()
        {
            if (_tabBar == null)
            {
                _tabBar = new Components.TabBar(_assetProviderService.Icons);

                _footerStackMediator.AddToFooter(_tabBar);
            }

            _tabBar.Bind(this);
        }

        public void Set(ICollection<TabBarItemViewModel> tabs) =>
            _tabBarItems.Set(tabs);

        public void Show() =>
            _isTabBarVisible.Value = true;

        public void Hide() =>
            _isTabBarVisible.Value = false;
    }
}