using Cysharp.Threading.Tasks;
using Source.Features.Quizzes.ActiveQuiz.VIewModels;
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

namespace Source.Features.Quizzes.ActiveQuiz.Components
{
    internal class QuizFinishedDialog : Dialog
    {
        private readonly CustomButton _primaryActionButton;
        private readonly CustomButton _secondaryActionButton;

        private CompositeDisposable _disposable;

        public QuizFinishedDialog(VisualElement root, QuizFinishedDialogViewModel viewModel, IIconProvider iconProvider) : base(root)
        {
            var backdrop = new VisualElement { name = "Backdrop" };
            backdrop.AddToClassList("dialog");

            Card = new VisualElement { name = "Card" };
            Card.AddToClassList("dialog__content");

            var iconElement = new VisualElement { name = "Icon" };
            iconElement.AddToClassList("dialog__icon");
            iconElement.AddToClassList("dialog__icon--warning");
            iconElement.style.backgroundImage = new StyleBackground(iconProvider.Get(Icons.Confetti));

            var body = new VisualElement { name = "Body" };
            body.AddToClassList("dialog__body");

            var titleLabel = new CustomLabel
            {
                name = "Title",
                Variant = CustomLabel.TextVariant.Title3,
                Weight = CustomLabel.FontWeight.Bold,
                Alignment = TextAnchor.UpperCenter,
                text = "Quiz Completed!",
                style =
                {
                    marginBottom = new StyleLength(8)
                }
            };

            var descriptionLabel = new CustomLabel
            {
                name = "Description",
                Variant = CustomLabel.TextVariant.Regular,
                Weight = CustomLabel.FontWeight.Regular,
                Alignment = TextAnchor.UpperCenter,
                text = "Great job! Your answers have been saved. Ready to see your performance and mastery updates?"
            };

            body.Add(titleLabel);
            body.Add(descriptionLabel);

            var actions = new VisualElement { name = "Actions" };
            actions.AddToClassList("dialog__actions");

            _primaryActionButton = new CustomButton
            {
                name = "PrimaryButton",
                Variant = CustomButton.ButtonVariant.Primary,
                Text = "View Analytics"
            };
            _primaryActionButton.AddToClassList("dialog__button");

            _secondaryActionButton = new CustomButton
            {
                name = "SecondaryButton",
                Variant = CustomButton.ButtonVariant.Ghost,
                Text = "Back to Quizzes"
            };
            _secondaryActionButton.AddToClassList("dialog__button");

            actions.Add(_primaryActionButton);
            actions.Add(_secondaryActionButton);

            Card.Add(iconElement);
            Card.Add(body);
            Card.Add(actions);
            backdrop.Add(Card);

            root.Add(backdrop);

            Bind(viewModel);

            Show().Forget();
        }

        private void Bind(QuizFinishedDialogViewModel viewModel)
        {
            _disposable = new CompositeDisposable();
            _primaryActionButton
                .Bind(SyncCommand.Create(() => HideAndExecute(viewModel.PrimaryActionCommand).Forget()))
                .AddTo(_disposable);

            _secondaryActionButton
                .Bind(SyncCommand.Create(() => HideAndExecute(viewModel.SecondaryActionCommand).Forget()))
                .AddTo(_disposable);

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