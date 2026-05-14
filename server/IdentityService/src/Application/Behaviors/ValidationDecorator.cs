using Application.Abstractions.Messaging;
using ErrorOr;
using FluentValidation;
using FluentValidation.Results;

namespace Application.Behaviors;

internal static class ValidationDecorator
{
    internal sealed class CommandHandler<TCommand, TResponse>(
        ICommandHandler<TCommand, TResponse> innerHandler,
        IEnumerable<IValidator<TCommand>> validators
    ) : ICommandHandler<TCommand, TResponse> where TCommand : ICommand<TResponse>
    {
        public async Task<ErrorOr<TResponse>> HandleAsync(TCommand command, CancellationToken cancellationToken)
        {
            var errors = await ValidateAsync(command, validators);

            if (errors.Count > 0)
            {
                return errors;
            }

            return await innerHandler.HandleAsync(command, cancellationToken);
        }
    }

    internal sealed class CommandBaseHandler<TCommand>(
        ICommandHandler<TCommand> innerHandler,
        IEnumerable<IValidator<TCommand>> validators
    ) : ICommandHandler<TCommand> where TCommand : ICommand
    {
        public async Task<ErrorOr<Success>> HandleAsync(TCommand command, CancellationToken cancellationToken)
        {
            var errors = await ValidateAsync(command, validators);

            if (errors.Count > 0)
            {
                return errors;
            }

            return await innerHandler.HandleAsync(command, cancellationToken);
        }
    }

    private static async Task<List<Error>> ValidateAsync<TCommand>(
        TCommand command,
        IEnumerable<IValidator<TCommand>> validators
    )
    {
        var enumerable = validators as IValidator<TCommand>[] ?? validators.ToArray();

        if (enumerable.Length == 0)
        {
            return [];
        }

        var context = new ValidationContext<TCommand>(command);

        ValidationResult[] validationResults = await Task.WhenAll(
            enumerable.Select(validator => validator.ValidateAsync(context)));

        var errors = validationResults
            .Where(validationResult => !validationResult.IsValid)
            .SelectMany(validationResult => validationResult.Errors)
            .Select(f => Error.Validation(
                code: f.PropertyName,
                description: f.ErrorMessage))
            .ToList();

        return errors;
    }
}