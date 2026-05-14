using Cysharp.Threading.Tasks;
using Source.Features.Auth.SigIn.Factory;
using Source.Features.Auth.SigIn.ViewModels;
using Source.Shared;
using Source.Shared.Components.Screens;
using Source.Shared.Extensions;
using Source.Shared.Reactive.Commands;
using Source.Shared.UIStack.Mediator;

namespace Source.Features.Auth.SigIn.Mediator
{
    internal class SignInScreenMediator : ISignInMediator
    {
        private readonly ISignInUIFactory _signInUIFactory;
        private readonly IUIStackMediator _stackMediator;

        public SignInScreenMediator(
            ISignInUIFactory signInUIFactory,
            IUIStackMediator uiStackMediator
        )
        {
            _signInUIFactory = signInUIFactory;
            _stackMediator = uiStackMediator;
        }

        public async UniTask<Result> CreateSignInScreen(string scope, ISignInScreenViewModel viewModel)
        {
            return await _signInUIFactory
                .CreateSignInScreen(scope, viewModel)
                .Tap(result => _stackMediator.Push(result));
        }
    }
}