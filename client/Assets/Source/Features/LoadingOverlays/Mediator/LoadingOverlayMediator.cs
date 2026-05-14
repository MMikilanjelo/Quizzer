using System;
using Cysharp.Threading.Tasks;
using Source.Features.LoadingOverlays.Factory;
using Source.Shared;
using Source.Shared.Components.Layouts;
using Source.Shared.Components.Overlays;
using Source.Shared.Extensions;
using Source.Shared.Services;
using Source.Shared.UIStack.Mediator;

namespace Source.Features.LoadingOverlays.Mediator
{
    internal class LoadingOverlayMediator : ILoadingOverlayMediator
    {
        private readonly ILoadingOverlayFactory _factory;
        private readonly IOverlayStackMediator _overlayStackMediator;

        public LoadingOverlayMediator(
            ILoadingOverlayFactory factory,
            IOverlayStackMediator overlayStackMediator
        )
        {
            _factory = factory;
            _overlayStackMediator = overlayStackMediator;
        }

        public async UniTask<Result> ShowLoadingOverlayAsync(string scope, Func<IProgress<float>, UniTask> loadingTask)
        {
            var viewResult = await _factory.CreateLoadingOverlayView(scope);

            return await viewResult.Match(
                onSuccess: async view =>
                {
                    _overlayStackMediator.Push(view);

                    var progressHandler = new Progress<float>(view.SetProgress);

                    try
                    {
                        await loadingTask(progressHandler);
                    }
                    finally
                    {
                        _overlayStackMediator.PopOverlay();
                    }

                    return Result.Success();
                },
                onFailure: error => error
            );
        }
    }
}