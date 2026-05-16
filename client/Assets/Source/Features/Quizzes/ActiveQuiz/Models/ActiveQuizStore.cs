namespace Source.Features.Quizzes.ActiveQuiz.Models
{
    public interface IActiveQuizStore
    {
        ActiveQuizModel Get();
        void Save(ActiveQuizModel quiz);
        void Clear();
    }

    internal sealed class ActiveQuizStore : IActiveQuizStore
    {
        private ActiveQuizModel _data;
        public ActiveQuizModel Get() => _data;
        public void Save(ActiveQuizModel quiz) => _data = quiz;
        public void Clear() => _data = null;
    }
}