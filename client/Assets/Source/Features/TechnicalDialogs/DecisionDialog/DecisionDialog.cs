using Cysharp.Threading.Tasks;
using Source.Shared.Components.Dialogs;
using Source.Shared.Components.Elements.Button;
using Source.Shared.Components.Elements.Label;
using Source.Shared.Reactive;
using Source.Shared.Reactive.Commands;
using Source.Shared.Reactive.Disposables;
using UnityEngine;
using UnityEngine.UIElements;

namespace Source.Features.TechnicalDialogs.DecisionDialog
{
    internal class DecisionDialog : Dialog
    {
        private readonly CustomButton _primaryActionButton;
        private readonly CustomButton _secondaryActionButton;
        private readonly CustomLabel _titleLabel;
        private readonly CustomLabel _descriptionLabel;

        private CompositeDisposable _disposable;

        public DecisionDialog(VisualElement root) : base(root)
        {
            var backdrop = new VisualElement { name = "Backdrop" };
            backdrop.AddToClassList("dialog");

            Card = new VisualElement { name = "Card" };
            Card.AddToClassList("dialog__content");

            var iconElement = new VisualElement { name = "Icon" };
            iconElement.AddToClassList("dialog__icon");
            iconElement.AddToClassList("dialog__icon--default");
            iconElement.style.backgroundImage = new StyleBackground();

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
                Text = "Retry"
            };
            _primaryActionButton.AddToClassList("dialog__button");

            _secondaryActionButton = new CustomButton
            {
                name = "SecondaryButton",
                Variant = CustomButton.ButtonVariant.Ghost,
                Text = "Cancel"
            };
            _secondaryActionButton.AddToClassList("dialog__button");

            actions.Add(_primaryActionButton);
            actions.Add(_secondaryActionButton);

            Card.Add(iconElement);
            Card.Add(body);
            Card.Add(actions);
            backdrop.Add(Card);

            root.Add(backdrop);
        }

        public void Bind(DecisionDialogViewModel viewModel)
        {
            _disposable = new CompositeDisposable();
            _primaryActionButton
                .Bind(SyncCommand.Create(() => HideAndExecute(viewModel.PrimaryActionCommand).Forget()))
                .AddTo(_disposable);

            _secondaryActionButton
                .Bind(SyncCommand.Create(() => HideAndExecute(viewModel.SecondaryActionCommand).Forget()))
                .AddTo(_disposable);

            _titleLabel.text = viewModel.Props.Title.ToString();
            _descriptionLabel.text = viewModel.Props.Message.ToString();

            _primaryActionButton.pickingMode = PickingMode.Position;
            _secondaryActionButton.pickingMode = PickingMode.Position;
        }

        public override void Dispose()
        {
            _disposable?.Dispose();
            _disposable = null;
        }

        private async UniTask HideAndExecute(ICommand command)
        {
            _primaryActionButton.pickingMode = PickingMode.Ignore;
            _secondaryActionButton.pickingMode = PickingMode.Ignore;
            await Hide();
            command.Execute().Forget();
        }
    }
}