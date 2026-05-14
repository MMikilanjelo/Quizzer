using System;
using System.Collections.Generic;
using Source.Features.Quizzes.MyQuizzes.ViewModels;
using Source.Shared.Components;
using Source.Shared.Components.Elements.Button;
using Source.Shared.Components.Elements.Label;
using Source.Shared.Reactive;
using Source.Shared.Reactive.Disposables;
using Source.Shared.Services;
using UnityEngine;
using UnityEngine.UIElements;

namespace Source.Features.Quizzes.MyQuizzes.Components
{
    public class NoQuizzesFound : VisualElement
    {
        private readonly VisualElement _iconElement;
        private readonly CustomLabel _titleLabel;
        private readonly CustomLabel _descriptionLabel;
        private readonly IIconProvider _iconProvider;

        private CompositeDisposable _disposable;

        public NoQuizzesFound(IIconProvider iconProvider)
        {
            _iconProvider = iconProvider;

            AddToClassList("no-quizzes");

            _iconElement = new VisualElement { name = "NoQuizzesIcon" };
            _iconElement.AddToClassList("no-quizzes__icon");

            _titleLabel = new CustomLabel
            {
                Variant = CustomLabel.TextVariant.Title3,
                Weight = CustomLabel.FontWeight.Bold,
                Alignment = TextAnchor.MiddleCenter
            };
            _titleLabel.AddToClassList("no-quizzes__title");

            _descriptionLabel = new CustomLabel
            {
                Variant = CustomLabel.TextVariant.Regular,
                Weight = CustomLabel.FontWeight.Regular,
                Alignment = TextAnchor.MiddleCenter
            };
            _descriptionLabel.AddToClassList("no-quizzes__description");

            Add(_iconElement);
            Add(_titleLabel);
            Add(_descriptionLabel);
        }

        public IDisposable Bind(NoQuizzesViewModel viewModel)
        {
            _disposable?.Dispose();
            _disposable = new CompositeDisposable();

            viewModel.Icon.Subscribe(OnIconChanged).AddTo(_disposable);
            viewModel.Title.Subscribe(title => _titleLabel.text = title).AddTo(_disposable);
            viewModel.Message.Subscribe(message => _descriptionLabel.text = message).AddTo(_disposable);

            _titleLabel.text = viewModel.Title.Value;
            _descriptionLabel.text = viewModel.Message.Value;
            OnIconChanged(viewModel.Icon.Value);

            return _disposable;
        }

        private void OnIconChanged(SpriteAtlasIconModel icon)
        {
            var sprite = _iconProvider.Get(icon);
            _iconElement.style.backgroundImage = new StyleBackground(sprite);
        }
    }
}