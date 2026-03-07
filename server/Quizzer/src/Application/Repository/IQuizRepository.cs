using Domain.Quizzes;

namespace Application.Repository;

public interface IQuizRepository
{
    Task CreateAsync(Quiz quiz);
}