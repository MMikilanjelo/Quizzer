using System;
using System.Collections.Generic;
using UnityEngine;

namespace Source.Features.Quizzes.CreateQuiz.Models
{
    public enum QuizCreationMode
    {
        Smart,
        Manual
    }

    public record QuizCreationModel
    {
        public QuizConfigurationModel Configuration { get; set; } = new();
        public string SelectedDomain { get; set; } = string.Empty;
        public string SelectedDifficulty { get; set; } = string.Empty;
        public QuizCreationMode Mode { get; set; } = QuizCreationMode.Smart;

        public int RequestedQuestions
        {
            get => Mathf.Clamp(_requestedQuestions, Configuration.MinQuestions, Configuration.MaxQuestions);
            set => _requestedQuestions = value;
        }

        private int _requestedQuestions = 15;
    }

    public record QuizConfigurationModel
    {
        public int MinQuestions { get; set; }
        public int MaxQuestions { get; set; }
        public IReadOnlyList<string> Difficulties { get; set; } = Array.Empty<string>();
        public IReadOnlyList<string> Domains { get; set; } = Array.Empty<string>();
    }
}