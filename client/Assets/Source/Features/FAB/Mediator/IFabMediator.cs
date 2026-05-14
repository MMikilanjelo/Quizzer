using System.Collections.Generic;
using Source.Features.FAB.ViewModels;
using Source.Shared.Reactive.Commands;
using Source.Shared.Reactive.Events;
using Source.Shared.Reactive.List;

namespace Source.Features.FAB.Mediator
{
    public interface IFabMediator
    {
        IReadOnlyReactiveProperty<bool> IsFabOpen { get; }
        IReadOnlyReactiveProperty<bool> IsFabVisible { get; }
        ICommand ToggleFabCommand { get; }
        ICommand CloseFabCommand { get; }
        IReadOnlyReactiveList<FabActionViewModel> FabMenuActions { get; }
        void Set(ICollection<FabActionViewModel> actions);
        void Initialize();
        void Clear();
    }
}