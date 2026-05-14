namespace Source.Features.Quizzes.MyQuizzes.Models
{
    public enum QuizFilter
    {
        All = 0,
        Pending = 1,
        Active = 2,
        Completed = 3
    }

    public record QuizFilterModel(QuizFilter Filter)
    {
        public QuizFilter Filter { get; } = Filter;
    }
}