using Application.Abstractions.Messaging;
using ErrorOr;
using Microsoft.Extensions.Logging;
using Serilog.Context;

namespace Application.Behaviors;

internal static partial class LoggingDecorator
{
    internal sealed class CommandHandler<TCommand, TResponse>(
        ICommandHandler<TCommand, TResponse> innerHandler,
        ILogger<CommandHandler<TCommand, TResponse>> logger
    ) : ICommandHandler<TCommand, TResponse> where TCommand : ICommand<TResponse>
    {
        public async Task<ErrorOr<TResponse>> HandleAsync(TCommand command, CancellationToken cancellationToken)
        {
            string commandName = typeof(TCommand).Name;

            LogProcessingCommand(logger, commandName);

            ErrorOr<TResponse> result = await innerHandler.HandleAsync(command, cancellationToken);

            if (!result.IsError)
            {
                LogCompletedCommand(logger, commandName);
            }
            else
            {
                using (LogContext.PushProperty("Errors", result.Errors, true))
                {
                    LogCommandError(logger, commandName);
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

            LogProcessingCommand(logger, commandName);

            ErrorOr<Success> result = await innerHandler.HandleAsync(command, cancellationToken);

            if (!result.IsError)
            {
                LogCompletedCommand(logger, commandName);
            }
            else
            {
                using (LogContext.PushProperty("Errors", result.Errors, true))
                {
                    LogCommandError(logger, commandName);
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

            LogProcessingQuery(logger, queryName);

            ErrorOr<TResponse> result = await innerHandler.Handle(query, cancellationToken);

            if (!result.IsError)
            {
                LogCompletedQuery(logger, queryName);
            }
            else
            {
                using (LogContext.PushProperty("Errors", result.Errors, true))
                {
                    LogQueryError(logger, queryName);
                }
            }

            return result;
        }
    }

    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "Processing command {Command}")]
    private static partial void LogProcessingCommand(ILogger logger, string command);

    [LoggerMessage(EventId = 2, Level = LogLevel.Information, Message = "Completed command {Command}")]
    private static partial void LogCompletedCommand(ILogger logger, string command);

    [LoggerMessage(EventId = 3, Level = LogLevel.Error, Message = "Completed command {Command} with error(s)")]
    private static partial void LogCommandError(ILogger logger, string command);

    [LoggerMessage(EventId = 4, Level = LogLevel.Information, Message = "Processing query {Query}")]
    private static partial void LogProcessingQuery(ILogger logger, string query);

    [LoggerMessage(EventId = 5, Level = LogLevel.Information, Message = "Completed query {Query}")]
    private static partial void LogCompletedQuery(ILogger logger, string query);

    [LoggerMessage(EventId = 6, Level = LogLevel.Error, Message = "Completed query {Query} with error(s)")]
    private static partial void LogQueryError(ILogger logger, string query);
}
