using Source.Shared.Components;
using Source.Shared.Icons;
using Source.Shared.Reactive.Commands;
using Source.Shared.Services;

namespace Source.Features.TechnicalDialogs.ErrorDialog
{
    public class ErrorDialogViewModel
    {
        public static SpriteAtlasIconModel Icon => Icons.WarningCircle;
        public ICommand PrimaryActionCommand { get; }
        public string Message => _model.Message;
        public string Title => _model.Title;

        private readonly ErrorModel _model;

        public ErrorDialogViewModel(
            ICommand primaryActionCommand,
            ErrorModel errorModel
        )
        {
            PrimaryActionCommand = primaryActionCommand;
            _model = errorModel;
        }
    }
}