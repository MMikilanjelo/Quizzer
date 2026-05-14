using Cysharp.Threading.Tasks;
using Source.Features.Auth.SigIn.Components;
using Source.Features.Auth.SigIn.Mediator;
using Source.Features.Auth.SigIn.ViewModels;
using Source.Shared;
using Source.Shared.Services;

namespace Source.Features.Auth.SigIn.Factory
{
    internal interface ISignInUIFactory
    {
        UniTask<Result<SignInScreen>> CreateSignInScreen(string scope, ISignInScreenViewModel viewModel);
    }
}