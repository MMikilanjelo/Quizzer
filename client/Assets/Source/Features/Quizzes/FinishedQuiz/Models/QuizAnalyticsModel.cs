using System.Collections.Generic;

namespace Source.Features.Quizzes.FinishedQuiz.Models
{
    public sealed record QuizAnalyticsModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public IReadOnlyList<string> Topics { get; set; }
        public int TotalQuestions { get; set; }
        public int AnsweredCount { get; set; }
        public int CorrectCount { get; set; }
        public int IncorrectCount { get; set; }
        public IReadOnlyList<ConceptMasteryDeltaModel> MasteryChanges { get; set; }
    }

    public sealed record ConceptMasteryDeltaModel
    {
        public string ConceptId { get; set; }
        public double StartingMastery { get; set; }
        public double EndingMastery { get; set; }
        public int Attempts { get; set; }
    }
}