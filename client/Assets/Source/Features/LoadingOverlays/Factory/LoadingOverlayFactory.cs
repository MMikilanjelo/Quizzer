using Cysharp.Threading.Tasks;
using Source.Features.LoadingOverlays.Components;
using Source.Shared;
using Source.Shared.Extensions;
using Source.Shared.Services;
using UnityEngine.UIElements;

namespace Source.Features.LoadingOverlays.Factory
{
    internal class LoadingOverlayFactory : ILoadingOverlayFactory
    {
        private readonly IAssetProviderService _assetProviderService;

        internal LoadingOverlayFactory(IAssetProviderService assetProviderService)
        {
            _assetProviderService = assetProviderService;
        }

        public async UniTask<Result<LoadingOverlay>> CreateLoadingOverlayView(string scope)
        {
            var result = await _assetProviderService.LoadAsset<VisualTreeAsset>("LoadingOverlay", scope);

            return result.Map(asset => new LoadingOverlay(asset.Instantiate()));
        }
    }
}