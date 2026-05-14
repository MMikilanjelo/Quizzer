using Source.Shared.Reactive.Events;

namespace Source.Features.Quizzes.FinishedQuiz.ViewModels
{
    public class PerformanceViewModel
    {
        public ReactiveProperty<int> CorrectCount { get; } = new();
        public ReactiveProperty<int> IncorrectCount { get; } = new();
        public ReactiveProperty<float> CorrectNormalized { get; } = new();
        public ReactiveProperty<float> IncorrectNormalized { get; } = new();
    }
}