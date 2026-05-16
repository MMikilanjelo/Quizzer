using System;
using Cysharp.Threading.Tasks;
using Source.Features.Quizzes.ActiveQuiz.VIewModels;
using Source.Shared.Components.Elements.CustomVisualElement;
using Source.Shared.Components.Elements.Label;
using Source.Shared.Extensions;
using Source.Shared.Reactive;
using Source.Shared.Reactive.Disposables;
using UnityEngine.UIElements;

namespace Source.Features.Quizzes.ActiveQuiz.Components
{
    public class QuizOption : ReactiveVisualElement
    {
        private readonly CustomLabel _indicatorLabel;
        private readonly CustomLabel _textLabel;

        private CompositeDisposable _disposable;

        public QuizOption()
        {
            AddToClassList("quiz-option");

            var indicatorBox = new VisualElement();
            indicatorBox.AddToClassList("quiz-option__indicator");

            _indicatorLabel = new CustomLabel
            {
                Variant = CustomLabel.TextVariant.Small,
                Weight = CustomLabel.FontWeight.Bold
            };
            _indicatorLabel.AddToClassList("quiz-option__letter");

            var dotElement = new VisualElement();
            dotElement.AddToClassList("quiz-option__dot");

            indicatorBox.Add(_indicatorLabel);
            indicatorBox.Add(dotElement);

            _textLabel = new CustomLabel
            {
                Variant = CustomLabel.TextVariant.Regular,
                Weight = CustomLabel.FontWeight.Regular,
                style = { whiteSpace = WhiteSpace.Normal, flexShrink = 1 }
            };
            _textLabel.AddToClassList("quiz-option__text");

            Add(indicatorBox);
            Add(_textLabel);
        }

        public IDisposable Bind(QuizOptionViewModel viewModel)
        {
            _disposable?.Dispose();
            _disposable = new CompositeDisposable();

            _indicatorLabel.text = viewModel.LetterIndicator;
            _textLabel.text = viewModel.Text;

            viewModel.IsSelected.Subscribe(isSelected =>
                {
                    if (isSelected)
                        AddToClassList("quiz-option--selected");
                    else
                        RemoveFromClassList("quiz-option--selected");
                })
                .AddTo(_disposable);

            return _disposable;
        }
    }
}