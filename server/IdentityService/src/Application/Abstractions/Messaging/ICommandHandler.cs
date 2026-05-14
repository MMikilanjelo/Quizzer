using ErrorOr;

namespace Application.Abstractions.Messaging;

public interface ICommandHandler<in TCommand> where TCommand : ICommand
{
    Task<ErrorOr<Success>> HandleAsync(TCommand command, CancellationToken cancellationToken);
}

public interface ICommandHandler<in TCommand, TResponse> where TCommand : ICommand<TResponse>
{
    Task<ErrorOr<TResponse>> HandleAsync(TCommand command, CancellationToken cancellationToken);
}