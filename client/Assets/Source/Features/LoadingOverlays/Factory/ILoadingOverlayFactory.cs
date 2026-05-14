using Cysharp.Threading.Tasks;
using Source.Features.LoadingOverlays.Components;
using Source.Shared;
using Source.Shared.Services;

namespace Source.Features.LoadingOverlays.Factory
{
    internal interface ILoadingOverlayFactory
    {
        UniTask<Result<LoadingOverlay>> CreateLoadingOverlayView(string scope);
    }
}