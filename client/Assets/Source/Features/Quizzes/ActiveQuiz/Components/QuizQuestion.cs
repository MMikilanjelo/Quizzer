using System;
using Source.Features.Quizzes.ActiveQuiz.VIewModels;
using Source.Shared.Components.Elements.Label;
using Source.Shared.Reactive.Events;
using UnityEngine.UIElements;

namespace Source.Features.Quizzes.ActiveQuiz.Components
{
    public class QuizQuestion : VisualElement
    {
        private readonly CustomLabel _questionTextLabel;

        public QuizQuestion()
        {
            AddToClassList("quiz-question");

            var container = new VisualElement();

            _questionTextLabel = new CustomLabel
            {
                Variant = CustomLabel.TextVariant.Regular,
                Weight = CustomLabel.FontWeight.SemiBold,
                style = { whiteSpace = WhiteSpace.Normal }
            };

            container.Add(_questionTextLabel);

            Add(container);
        }

        public IDisposable Bind(IReadOnlyReactiveProperty<string> viewModel)
        {
            _questionTextLabel.text = viewModel.Value;

            return viewModel.Subscribe(text => _questionTextLabel.text = text);
        }
    }
}