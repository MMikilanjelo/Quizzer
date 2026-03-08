using Application.Repository;
using Domain.Quizzes;
using MongoDB.Driver;

namespace Infrastructure.Repository;

public class QuizRepository(IMongoDatabase database) : IQuizRepository
{
    private readonly IMongoCollection<Quiz> _collection = database.GetCollection<Quiz>(nameof(Quiz));

    public Task CreateAsync(Quiz quiz)
    {
        return _collection.InsertOneAsync(quiz);
    }
}