using Application.Abstractions.Messaging;
using ErrorOr;
using Microsoft.Extensions.Logging;
using Serilog.Context;

namespace Application.Behaviors;

internal static class LoggingDecorator
{
    internal sealed class CommandHandler<TCommand, TResponse>(
        ICommandHandler<TCommand, TResponse> innerHandler,
        ILogger<CommandHandler<TCommand, TResponse>> logger
    ) : ICommandHandler<TCommand, TResponse> where TCommand : ICommand<TResponse>
    {
        public async Task<ErrorOr<TResponse>> HandleAsync(TCommand command, CancellationToken cancellationToken)
        {
            string commandName = typeof(TCommand).Name;

            logger.LogInformation("Processing command {Command}", commandName);

            ErrorOr<TResponse> result = await innerHandler.HandleAsync(command, cancellationToken);

            if (!result.IsError)
            {
                logger.LogInformation("Completed command {Command}", commandName);
            }
            else
            {
                using (LogContext.PushProperty("Errors", result.Errors, true))
                {
                    logger.LogError("Completed command {Command} with error(s)", commandName);
                }
            }

            return result;
        }
    }

    internal sealed class CommandBaseHandler<TCommand>(
        ICommandHandler<TCommand> innerHandler,
        ILogger<CommandBaseHandler<TCommand>> logger
    ) : ICommandHandler<TCommand> where TCommand : ICommand
    {
        public async Task<ErrorOr<Success>> HandleAsync(TCommand command, CancellationToken cancellationToken)
        {
            string commandName = typeof(TCommand).Name;

            logger.LogInformation("Processing command {Command}", commandName);

            ErrorOr<Success> result = await innerHandler.HandleAsync(command, cancellationToken);

            if (!result.IsError)
            {
                logger.LogInformation("Completed command {Command}", commandName);
            }
            else
            {
                using (LogContext.PushProperty("Errors", result.Errors, true))
                {
                    logger.LogError("Completed command {Command} with error(s)", commandName);
                }
            }

            return result;
        }
    }

    internal sealed class QueryHandler<TQuery, TResponse>(
        IQueryHandler<TQuery, TResponse> innerHandler,
        ILogger<QueryHandler<TQuery, TResponse>> logger
    ) : IQueryHandler<TQuery, TResponse> where TQuery : IQuery<TResponse>
    {
        public async Task<ErrorOr<TResponse>> Handle(TQuery query, CancellationToken cancellationToken)
        {
            string queryName = typeof(TQuery).Name;

            logger.LogInformation("Processing query {Query}", queryName);

            ErrorOr<TResponse> result = await innerHandler.Handle(query, cancellationToken);

            if (!result.IsError)
            {
                logger.LogInformation("Completed query {Query}", queryName);
            }
            else
            {
                using (LogContext.PushProperty("Errors", result.Errors, true))
                {
                    logger.LogError("Completed query {Query} with error(s)", queryName);
                }
            }

            return result;
        }
    }
}