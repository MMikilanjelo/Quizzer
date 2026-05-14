using Source.Shared.Reactive.Commands;

namespace Source.Features.TechnicalDialogs.DecisionDialog
{
    public class DecisionDialogViewModel
    {
        public DecisionDialogProps Props { get; }
        public ICommand PrimaryActionCommand { get; }
        public ICommand SecondaryActionCommand { get; }

        public DecisionDialogViewModel(
            ICommand primaryActionCommand,
            ICommand secondaryActionCommand,
            DecisionDialogProps props
        )
        {
            PrimaryActionCommand = primaryActionCommand;
            SecondaryActionCommand = secondaryActionCommand;
            Props = props;
        }
    }
}