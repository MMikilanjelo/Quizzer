using System.Threading;
using Cysharp.Threading.Tasks;

namespace Source.Shared.UIStack.Mediator
{
    public interface IUIStackMediator :
        IScreenStackMediator,
        IDialogStackMediator,
        IOverlayStackMediator,
        IFooterStackMediator
    {
        UniTask<Result> CreateUIStack(string scope, CancellationToken cancellationToken = default);
        void PopAll();
    }
}