using System.Collections.Generic;

namespace Source.Features.Quizzes.CreateQuiz.Models
{
    public interface IQuizCreationRepository
    {
        QuizCreationModel Get();
        void SaveSelectedDomain(string domain);
        void SaveConfiguration(QuizConfigurationModel configuration);
        void SaveSelectedDifficulty(string difficulty);
        void SaveRequestedQuestions(int count);
        void SaveMode(QuizCreationMode mode);
        void Clear();
    }
}