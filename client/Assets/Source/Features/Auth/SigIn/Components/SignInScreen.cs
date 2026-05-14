using Source.Features.Auth.SigIn.ViewModels;
using Source.Shared.Components.Elements.Button;
using Source.Shared.Components.Layouts.ActionFooter;
using Source.Shared.Components.Screens;
using Source.Shared.Reactive;
using Source.Shared.Reactive.Disposables;
using UnityEngine;
using UnityEngine.UIElements;

namespace Source.Features.Auth.SigIn.Components
{
    internal class SignInScreen : IScreenView, IScreenWithFooterView
    {
        public VisualElement Root { get; }
        public VisualElement Footer { get; }

        private readonly CustomButton _letsGoButton;

        private readonly ISignInScreenViewModel _viewModel;

        private CompositeDisposable _disposable = new();

        public SignInScreen(
            TemplateContainer visualElement,
            Sprite heroImageSprite,
            ISignInScreenViewModel viewModel
        )
        {
            Root = visualElement;
            Root.AddToClassList("screen");

            var heroImage = new VisualElement();
            heroImage.AddToClassList("img-fullscreen");
            heroImage.style.backgroundImage = new StyleBackground(heroImageSprite);
            
            Root.Add(heroImage);

            var footer = new ActionFooter();

            _letsGoButton = new CustomButton
            {
                Text = "LetsGo",
                Variant = CustomButton.ButtonVariant.Primary
            };

            footer.AddAction(_letsGoButton);

            _viewModel = viewModel;

            Footer = footer;
        }

        public void Initialize()
        {
            _disposable = new CompositeDisposable();

            _letsGoButton
                .Bind(_viewModel.SignUpCommand)
                .AddTo(_disposable);
        }

        public void Dispose() =>
            _disposable?.Dispose();
    }
}