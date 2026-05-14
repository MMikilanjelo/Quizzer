using Cysharp.Threading.Tasks;
using Source.Shared.Components.Dialogs;
using Source.Shared.Components.Elements.Button;
using Source.Shared.Components.Elements.Label;
using Source.Shared.Icons;
using Source.Shared.Reactive;
using Source.Shared.Reactive.Commands;
using Source.Shared.Reactive.Disposables;
using Source.Shared.Services;
using UnityEngine;
using UnityEngine.UIElements;

namespace Source.Features.TechnicalDialogs.ErrorDialog
{
    internal class ErrorDialog : Dialog
    {
        private readonly CustomButton _primaryActionButton;
        private readonly CustomLabel _titleLabel;
        private readonly CustomLabel _descriptionLabel;
        private readonly VisualElement _icon;

        private CompositeDisposable _disposable;
        private readonly IIconProvider _iconProvider;

        public ErrorDialog(VisualElement root , IIconProvider iconProvider) : base(root)
        {
            _iconProvider = iconProvider;
            
            var backdrop = new VisualElement { name = "Backdrop" };
            backdrop.AddToClassList("dialog");

            Card = new VisualElement { name = "Card" };
            Card.AddToClassList("dialog__content");

            _icon = new VisualElement { name = "Icon" };
            _icon.AddToClassList("dialog__icon");
            _icon.AddToClassList("dialog__icon--error");
            _icon.style.backgroundImage = new StyleBackground();

            var body = new VisualElement { name = "Body" };
            body.AddToClassList("dialog__body");

            _titleLabel = new CustomLabel
            {
                name = "Title",
                Variant = CustomLabel.TextVariant.Title3,
                Weight = CustomLabel.FontWeight.Bold,
                Alignment = TextAnchor.UpperCenter,
                style =
                {
                    marginBottom = new StyleLength(8)
                }
            };

            _descriptionLabel = new CustomLabel
            {
                name = "Description",
                Variant = CustomLabel.TextVariant.Regular,
                Weight = CustomLabel.FontWeight.Regular,
                Alignment = TextAnchor.UpperCenter
            };

            body.Add(_titleLabel);
            body.Add(_descriptionLabel);

            var actions = new VisualElement { name = "Actions" };
            actions.AddToClassList("dialog__actions");

            _primaryActionButton = new CustomButton
            {
                name = "PrimaryButton",
                Variant = CustomButton.ButtonVariant.Primary,
                Text = "Close"
            };
            _primaryActionButton.AddToClassList("dialog__button");

            actions.Add(_primaryActionButton);

            Card.Add(_icon);
            Card.Add(body);
            Card.Add(actions);
            backdrop.Add(Card);

            root.Add(backdrop);
        }

        public void Bind(ErrorDialogViewModel viewModel)
        {
            _disposable = new CompositeDisposable();

            _primaryActionButton
                .Bind(SyncCommand.Create(() => HideAndExecute(viewModel.PrimaryActionCommand).Forget()))
                .AddTo(_disposable);

            _titleLabel.text = viewModel.Title;
            _descriptionLabel.text = viewModel.Message;
            _icon.style.backgroundImage = new StyleBackground(_iconProvider.Get(ErrorDialogViewModel.Icon));

            _primaryActionButton.pickingMode = PickingMode.Position;
            
            Show().Forget();
        }

        public override void Dispose()
        {
            _disposable?.Dispose();
            _disposable = null;
        }

        private async UniTask HideAndExecute(ICommand command)
        {
            _primaryActionButton.pickingMode = PickingMode.Ignore;

            await Hide();

            command.Execute().Forget();
        }
    }
}