using System.Collections.Generic;
using System.Linq;

namespace Source.Features.Quizzes.ActiveQuiz.Models
{
    public class ActiveQuizQuestionModel
    {
        public string Id { get; set; }
        public string Text { get; set; }
        public List<string> Options { get; set; }
    }

    public class ActiveQuizModel
    {
        public string Id { get; set; }
        public List<string> Topics { get; set; }
        public string Name { get; set; }
        public List<ActiveQuizQuestionModel> Questions { get; set; }
        public HashSet<string> AnsweredQuestionIds { get; set; }

        public void RecordAnswer(string questionId) =>
            AnsweredQuestionIds.Add(questionId);

        public ActiveQuizQuestionModel GetActiveQuestion() =>
            Questions.FirstOrDefault(q => !AnsweredQuestionIds.Contains(q.Id));

        public float GetProgress()
        {
            if (Questions.Count == 0)
            {
                return 0f;
            }

            return (float)AnsweredQuestionIds.Count / Questions.Count;
        }
    }
}