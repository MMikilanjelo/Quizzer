using System.Threading;
using Cysharp.Threading.Tasks;

namespace Source.Shared
{
    public interface IUseCase<in TRequest, TResponse>
    {
        UniTask<Result<TResponse>> Execute(TRequest request, CancellationToken cancellationToken = default);
    }

    public interface IUseCase<TResponse>
    {
        UniTask<Result<TResponse>> Execute(CancellationToken cancellationToken = default);
    }
}