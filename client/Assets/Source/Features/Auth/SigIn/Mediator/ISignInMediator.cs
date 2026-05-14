using Cysharp.Threading.Tasks;
using Source.Features.Auth.SigIn.ViewModels;
using Source.Shared;
using Source.Shared.Components.Screens;
using Source.Shared.Services;

namespace Source.Features.Auth.SigIn.Mediator
{
    public interface ISignInMediator
    {
        UniTask<Result> CreateSignInScreen(string scope, ISignInScreenViewModel viewModel);
    }
}