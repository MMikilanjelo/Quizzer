using System;
using Source.Shared.Reactive;
using Source.Shared.Reactive.Commands;
using Source.Shared.Reactive.Disposables;
using Source.Shared.Reactive.Events;
using UnityEngine;

namespace Source.Features.Quizzes.CreateQuiz.ViewModels
{
    public class SelectNumberOfQuestionViewModel : IDisposable
    {
        public ICommand IncrementCommand { get; }
        public ICommand DecrementCommand { get; }
        public IReadOnlyReactiveProperty<int> Value => _value;
        private readonly ReactiveProperty<int> _value;

        private readonly CompositeDisposable _disposable = new();

        public SelectNumberOfQuestionViewModel(int initialValue = 15, int min = 10, int max = 50, int step = 5)
        {
            _value = new ReactiveProperty<int>(Mathf.Clamp(initialValue, min, max));

            var canIncrement = new ReactiveProperty<bool>(_value.Value < max);
            var canDecrement = new ReactiveProperty<bool>(_value.Value > min);

            _value.Subscribe(val =>
            {
                canIncrement.Value = val < max;
                canDecrement.Value = val > min;
            }).AddTo(_disposable);

            IncrementCommand = SyncCommand.Create(() => { _value.Value = Mathf.Clamp(_value.Value + step, min, max); }).WithExecutionRule(canIncrement);
            DecrementCommand = SyncCommand.Create(() => { _value.Value = Mathf.Clamp(_value.Value - step, min, max); }).WithExecutionRule(canDecrement);
        }

        public void Dispose()
        {
            _disposable?.Dispose();
            IncrementCommand.Dispose();
            DecrementCommand.Dispose();
        }
    }
}