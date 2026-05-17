using System.Threading;
using Cysharp.Threading.Tasks;
using Source.Features.MyProfile.Components;
using Source.Features.MyProfile.ViewModels;
using Source.Shared;
using Source.Shared.Extensions;
using Source.Shared.Services;
using UnityEngine.UIElements;

namespace Source.Features.MyProfile.Factory
{
    internal class MyProfileFactory : IMyProfileFactory
    {
        private readonly IAssetProviderService _assetProviderService;

        public MyProfileFactory(IAssetProviderService assetProviderService) =>
            _assetProviderService = assetProviderService;

        public async UniTask<Result<MyProfileScreen>> CreateMyProfileScreen(IMyProfileScreenViewModel viewModel)
        {
            return await _assetProviderService
                .LoadAsset<StyleSheet>("MyProfileScreen", "Global", CancellationToken.None)
                .Bind(result =>
                {
                    var templateContainer = new TemplateContainer();

                    templateContainer.styleSheets.Add(result);

                    return Result<MyProfileScreen>.Success(new MyProfileScreen(templateContainer, viewModel, _assetProviderService.Icons));
                });
        }
    }
}