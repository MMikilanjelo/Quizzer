using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Source.Features.Quizzes.CreateQuiz.Models
{
    internal class QuizCreationRepository : IQuizCreationRepository
    {
        private QuizCreationModel _data = new();

        public QuizCreationModel Get() =>
            _data;

        public void SaveConfiguration(QuizConfigurationModel configuration) =>
            _data = _data with { Configuration = configuration };

        public void SaveSelectedDomain(string domain) =>
            _data = _data with { SelectedDomain = domain };

        public void SaveSelectedDifficulty(string difficulty) =>
            _data = _data with { SelectedDifficulty = difficulty };

        public void SaveRequestedQuestions(int count) =>
            _data = _data with { RequestedQuestions = count };

        public void SaveMode(QuizCreationMode mode) =>
            _data = _data with { Mode = mode };

        public void Clear() =>
            _data = new QuizCreationModel();
    }
}