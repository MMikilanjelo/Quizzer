using System;
using System.Collections.Generic;

namespace Source.Features.Quizzes.MyQuizzes.Models
{
    public sealed record QuizModel
    {
        public string Id { get; set; }
        public List<string> Topics { get; set; }
        public string Name { get; set; }
        public string Status { get; set; }
        public int QuestionCount { get; set; }
        public int AnsweredCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}