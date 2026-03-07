using Application.Abstractions.Messaging;
using Application.Authentication;
using Application.Repository;
using Domain.Quizzes;
using Domain.Users;
using ErrorOr;

namespace Application.Quizzes.Create;

public sealed record CreateQuizCommand : ICommand<Guid>;

internal sealed class CreateQuiz(
    IQuizRepository repository
) : ICommandHandler<CreateQuizCommand, Guid>
{
    public async Task<ErrorOr<Guid>> Handle(CreateQuizCommand command, CancellationToken cancellationToken)
    {
        var id = Guid.NewGuid();

        var quiz = new Quiz
        {
            Id = id.ToString()
        };

        await repository.CreateAsync(quiz);
        
        var id2 = Guid.NewGuid();

        var quiz2 = new Quiz
        {
            Id = id2.ToString()
        };

        await repository.CreateAsync(quiz2);
        
        return id;
    }
}