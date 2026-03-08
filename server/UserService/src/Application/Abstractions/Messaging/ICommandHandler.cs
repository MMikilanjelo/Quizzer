using ErrorOr;

namespace Application.Abstractions.Messaging;

public interface ICommandHandler<in TCommand> where TCommand : ICommand
{
    Task<ErrorOr<Success>> Handle(TCommand command, CancellationToken cancellationToken);
}

public interface ICommandHandler<in TCommand, TResponse> where TCommand : ICommand<TResponse>
{
    Task<ErrorOr<TResponse>> Handle(TCommand command, CancellationToken cancellationToken);
}