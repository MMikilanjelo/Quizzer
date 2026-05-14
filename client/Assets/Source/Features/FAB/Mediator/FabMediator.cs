using System.Collections.Generic;
using Source.Features.FAB.Components;
using Source.Features.FAB.ViewModels;
using Source.Shared.Reactive.Commands;
using Source.Shared.Reactive.Events;
using Source.Shared.Reactive.List;
using Source.Shared.Services;
using Source.Shared.UIStack.Mediator;
using UnityEngine;

namespace Source.Features.FAB.Mediator
{
    public class FabMediator : IFabMediator
    {
        public IReadOnlyReactiveProperty<bool> IsFabOpen => _isFabOpen;
        public IReadOnlyReactiveProperty<bool> IsFabVisible => _isFabVisible;
        public ICommand ToggleFabCommand { get; }
        public ICommand CloseFabCommand { get; }
        public IReadOnlyReactiveList<FabActionViewModel> FabMenuActions => _fabMenuActions;

        private readonly IOverlayStackMediator _overlayStackMediator;
        private readonly IAssetProviderService _assetProviderService;
        private readonly ReactiveProperty<bool> _isFabVisible = new(false);
        private readonly ReactiveProperty<bool> _isFabOpen = new(false);
        private readonly ReactiveList<FabActionViewModel> _fabMenuActions = new();

        private FloatingActionButton _fab;

        public FabMediator(IOverlayStackMediator overlayStackMediator, IAssetProviderService assetProviderService)
        {
            ToggleFabCommand = SyncCommand.Create(() => { _isFabOpen.Value = !_isFabOpen.Value; });
            CloseFabCommand = SyncCommand.Create(() => _isFabOpen.Value = false);
            _overlayStackMediator = overlayStackMediator;
            _assetProviderService = assetProviderService;

            _fabMenuActions.Added.Subscribe(_ => _isFabVisible.Value = true);
            _fabMenuActions.Removed.Subscribe(_ => { _isFabVisible.Value = _fabMenuActions.Count > 0; });

            _isFabOpen.Subscribe(value =>
            {
                if (value)
                {
                    _overlayStackMediator.ShowBackdrop();
                }
                else
                {
                    _overlayStackMediator.HideBackdrop();
                }
            });
        }

        public void Initialize()
        {
            if (_fab == null)
            {
                _fab = new FloatingActionButton(_assetProviderService.Icons);

                _overlayStackMediator.AddToOverlay(_fab);
            }

            _fab.Bind(this);
        }

        public void Set(ICollection<FabActionViewModel> actions)
        {
            _fabMenuActions.Clear();
            _fabMenuActions.AddRange(actions);
        }

        public void Clear() =>
            _fabMenuActions.Clear();
    }
}