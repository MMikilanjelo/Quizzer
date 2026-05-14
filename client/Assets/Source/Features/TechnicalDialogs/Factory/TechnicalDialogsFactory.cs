using Cysharp.Threading.Tasks;
using Source.Shared;
using Source.Shared.Extensions;
using Source.Shared.Services;
using UnityEngine;
using UnityEngine.UIElements;

namespace Source.Features.TechnicalDialogs.Factory
{
    internal class TechnicalDialogsFactory : ITechnicalDialogsFactory
    {
        private readonly IAssetProviderService _assetProviderService;

        internal TechnicalDialogsFactory(IAssetProviderService assetProviderService)
        {
            _assetProviderService = assetProviderService;
        }

        public DecisionDialog.DecisionDialog CreateDecisionDialogView() =>
            new((VisualElement)new TemplateContainer());

        public ErrorDialog.ErrorDialog CreateErrorDialog() =>
            new(new TemplateContainer() , _assetProviderService.Icons);
    }
}