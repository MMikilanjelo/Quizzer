using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Source.Features.Quizzes.CreateQuiz.Models
{
    public interface IQuizCreationStore
    {
        QuizCreationModel Get();
        void Clear();
        void Save(QuizCreationModel quizCreationModel);
    }

    internal class QuizCreationStore : IQuizCreationStore
    {
        private QuizCreationModel _data = new();

        public QuizCreationModel Get() =>
            _data;

        public void Save(QuizCreationModel quizCreationModel) =>
            _data = quizCreationModel;

        public void Clear() =>
            _data = new QuizCreationModel();
    }
}