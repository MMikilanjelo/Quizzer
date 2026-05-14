using Cysharp.Threading.Tasks;
using Source.Features.TechnicalDialogs.DecisionDialog;
using Source.Features.TechnicalDialogs.ErrorDialog;
using Source.Shared;
using Source.Shared.Components.Dialogs;

namespace Source.Features.TechnicalDialogs.Mediator
{
    public interface ITechnicalDialogsMediator
    {
        void CreateTryAgainDialog(DecisionDialogViewModel viewModel);
        void CreateErrorDialog(ErrorDialogViewModel viewModel);
    }
}