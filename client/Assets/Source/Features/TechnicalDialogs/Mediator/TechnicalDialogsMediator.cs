using Cysharp.Threading.Tasks;
using Source.Features.TechnicalDialogs.DecisionDialog;
using Source.Features.TechnicalDialogs.ErrorDialog;
using Source.Features.TechnicalDialogs.Factory;
using Source.Shared;
using Source.Shared.Components.Dialogs;
using Source.Shared.Components.Layouts;
using Source.Shared.Extensions;
using Source.Shared.Services;
using Source.Shared.UIStack.Mediator;

namespace Source.Features.TechnicalDialogs.Mediator
{
    internal class TechnicalDialogsMediator : ITechnicalDialogsMediator
    {
        private readonly IDialogStackMediator _dialogStackMediator;
        private readonly ITechnicalDialogsFactory _factory;

        internal TechnicalDialogsMediator(
            IDialogStackMediator dialogStackMediator,
            ITechnicalDialogsFactory factory
        )
        {
            _dialogStackMediator = dialogStackMediator;
            _factory = factory;
        }

        public void CreateTryAgainDialog(DecisionDialogViewModel viewModel)
        {
            var dialog = _factory.CreateDecisionDialogView();

            dialog.Bind(viewModel);

            _dialogStackMediator.Push(dialog);
        }

        public void CreateErrorDialog(ErrorDialogViewModel viewModel)
        {
            var dialog = _factory.CreateErrorDialog();

            dialog.Bind(viewModel);

            _dialogStackMediator.Push(dialog);
        }
    }
}