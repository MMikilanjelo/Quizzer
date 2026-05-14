using System;
using System.Collections.Generic;

namespace Source.Features.Quizzes.CreateQuiz.Models
{
    public record QuizConfigurationModel
    {
        public int MinQuestions { get; set; }
        public int MaxQuestions { get; set; }
        public IReadOnlyList<string> Difficulties { get; set; } = Array.Empty<string>();
        public IReadOnlyList<string> Domains { get; set; } = Array.Empty<string>();
    }
    
}