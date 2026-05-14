using Cysharp.Threading.Tasks;
using Source.Features.Auth.SigIn.Components;
using Source.Features.Auth.SigIn.Mediator;
using Source.Features.Auth.SigIn.ViewModels;
using Source.Shared;
using Source.Shared.Extensions;
using Source.Shared.Services;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UIElements;

namespace Source.Features.Auth.SigIn.Factory
{
    internal class SignInUIFactory : ISignInUIFactory
    {
        private readonly IAssetProviderService _assetProviderService;

        public SignInUIFactory(IAssetProviderService assetProviderService)
        {
            _assetProviderService = assetProviderService;
        }

        public UniTask<Result<SignInScreen>> CreateSignInScreen(string scope, ISignInScreenViewModel viewModel)
        {
            return _assetProviderService
                .LoadAsset<Sprite>("img_sig-in_background", scope)
                .Bind(result =>
                {
                    var templateContainer = new TemplateContainer();

                    return UniTask.FromResult(Result<SignInScreen>.Success(new SignInScreen(templateContainer, result, viewModel)));
                });
        }
    }
}