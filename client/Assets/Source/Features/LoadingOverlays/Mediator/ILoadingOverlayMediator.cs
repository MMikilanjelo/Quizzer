using System;
using Cysharp.Threading.Tasks;
using Source.Shared;
using Source.Shared.Services;

namespace Source.Features.LoadingOverlays.Mediator
{
    public interface ILoadingOverlayMediator
    {
        UniTask<Result> ShowLoadingOverlayAsync(string scope, Func<IProgress<float>, UniTask> loadingTask);
    }
}