using Source.Shared.Components.Dialogs;
using Source.Shared.Reactive.Commands;

namespace Source.Shared.UIStack.Mediator
{
    public interface IDialogStackMediator
    {
        void Push(IDialogView view);
        void PopDialog();
        void PopAllDialogs();
    }
}