using System;
using Source.Features.Quizzes.CreateQuiz.ViewModels;
using Source.Shared.Components.Elements.Button;
using Source.Shared.Components.Elements.Label;
using Source.Shared.Icons;
using Source.Shared.Reactive;
using Source.Shared.Reactive.Disposables;
using Source.Shared.Services;
using UnityEngine;
using UnityEngine.UIElements;

namespace Source.Features.Quizzes.CreateQuiz.Components
{
    public class SelectNumberOfQuestionView : VisualElement
    {
        private readonly CustomLabel _titleLabel;
        private readonly CustomButton _minusButton;
        private readonly CustomButton _plusButton;
        private readonly CustomLabel _numberLabel;
        private CompositeDisposable _disposable;

        public SelectNumberOfQuestionView(IIconProvider iconProvider)
        {
            var row = new VisualElement();

            row.AddToClassList("number-stepper__row");

            _minusButton = new CustomButton()
            {
                Size = CustomButton.ButtonSize.Large,
                Shape = CustomButton.ButtonShape.Circle,
                Icon = iconProvider.Get(Icons.MinusBold),
                Variant = CustomButton.ButtonVariant.Outline
            };

            var valueContainer = new VisualElement();
            valueContainer.AddToClassList("number-stepper__value-container");

            _numberLabel = new CustomLabel
            {
                Variant = CustomLabel.TextVariant.Title3,
                Weight = CustomLabel.FontWeight.SemiBold
            };
            _numberLabel.AddToClassList("number-stepper__number-label");

            var suffixLabel = new CustomLabel
            {
                Variant = CustomLabel.TextVariant.Tiny,
                Weight = CustomLabel.FontWeight.Regular,
                text = "Questions"
            };
            suffixLabel.AddToClassList("number-stepper__suffix-label");

            valueContainer.Add(_numberLabel);
            valueContainer.Add(suffixLabel);

            _plusButton = new CustomButton
            {
                Size = CustomButton.ButtonSize.Large,
                Shape = CustomButton.ButtonShape.Circle,
                Icon = iconProvider.Get(Icons.PlusBold),
                Variant = CustomButton.ButtonVariant.Outline
            };

            row.Add(_minusButton);
            row.Add(valueContainer);
            row.Add(_plusButton);
            Add(row);
        }

        public IDisposable Bind(SelectNumberOfQuestionViewModel model)
        {
            _disposable?.Dispose();
            _disposable = new CompositeDisposable();

            model.Value
                .Subscribe(val => _numberLabel.text = val.ToString())
                .AddTo(_disposable);

            _minusButton.Bind(model.DecrementCommand).AddTo(_disposable);
            _plusButton.Bind(model.IncrementCommand).AddTo(_disposable);

            _numberLabel.text = model.Value.ToString();

            return _disposable;
        }
    }
}