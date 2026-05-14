using Cysharp.Threading.Tasks;
using Source.Shared;
using Source.Shared.Services;

namespace Source.Features.TechnicalDialogs.Factory
{
    internal interface ITechnicalDialogsFactory
    {
        DecisionDialog.DecisionDialog CreateDecisionDialogView();
        ErrorDialog.ErrorDialog CreateErrorDialog();
    }
}