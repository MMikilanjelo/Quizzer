using System.Collections.Generic;
using Source.Features.TabBar.ViewModels;
using Source.Shared.Reactive.Commands;
using Source.Shared.Reactive.Events;
using Source.Shared.Reactive.List;

namespace Source.Features.TabBar.Mediator
{
    public interface ITabBarMediator
    {
        IReadOnlyReactiveList<TabBarItemViewModel> TabBarItems { get; }
        IReadOnlyReactiveProperty<bool> IsTabBarVisible { get; }
        IReadOnlyReactiveEvent<TabBarItemViewModel> TabSelectionChanged { get; }
        ICommand<TabBarItemViewModel> SelectTabCommand { get; }
        void Initialize();
        void Set(ICollection<TabBarItemViewModel> tabs);
        void Show();
        void Hide();
    }
}