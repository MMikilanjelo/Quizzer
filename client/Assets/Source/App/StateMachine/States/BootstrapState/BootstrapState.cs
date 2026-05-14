using System;
using Cysharp.Threading.Tasks;
using Source.Features.LoadingOverlays.Mediator;
using Source.Shared.Extensions;
using Source.Shared.Services;
using Source.Shared.StateMachine.States;
using Source.Shared.UIStack.Mediator;
using UnityEngine.U2D;

namespace Source.App.StateMachine.States.BootstrapState
{
    public class BootstrapState : GlobalState, IEnterState
    {
        private readonly ILoadingOverlayMediator _loadingOverlayMediator;
        private readonly IAssetProviderService _assetProviderService;
        private readonly IUIStackMediator _uiStackMediator;
        private static string Scope => nameof(BootstrapState);

        public BootstrapState(
            ILoadingOverlayMediator loadingOverlayMediator,
            IAssetProviderService assetProviderService,
            IUIStackMediator uiStackMediator
        ) 
        {
            _loadingOverlayMediator = loadingOverlayMediator;
            _assetProviderService = assetProviderService;
            _uiStackMediator = uiStackMediator;
        }

        public void Enter()
        {
            _uiStackMediator
                .CreateUIStack(Scope)
                .ContinueWith(_ =>
                {
                    _loadingOverlayMediator
                        .ShowLoadingOverlayAsync(Scope, WarmupAssets)
                        .ContinueWith(result => result.Switch(
                            onSuccess: () => { StateMachine.Enter<MainState.MainState>(); },
                            onFailure: error => { }
                        ))
                        .Forget();
                }).Forget();
        }

        private async UniTask WarmupAssets(IProgress<float> progress)
        {
            await _assetProviderService.WarmupAssetsByLabel("UIAssets", progress);

            await _assetProviderService.LoadAsset<SpriteAtlas>("LocalIcons", Scope);
        }
    }
}