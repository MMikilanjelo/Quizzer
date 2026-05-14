using System;
using Cysharp.Threading.Tasks;

namespace Source.Shared.Extensions
{
    public static class ResultExtensions
    {
        public static Result<T> Map<T>(this Result result, Func<T> mapping) =>
            result.IsFailure ? Result<T>.Failure(result.Error) : Result<T>.Success(mapping());

        public static Result<TOut> Map<T, TOut>(this Result<T> result, Func<T, TOut> mapping) =>
            result.IsFailure ? Result<TOut>.Failure(result.Error) : Result<TOut>.Success(mapping(result.Value));

        public static async UniTask<Result<TOut>> Map<T, TOut>(this Result<T> result, Func<T, UniTask<TOut>> mapping) =>
            result.IsFailure ? Result<TOut>.Failure(result.Error) : Result<TOut>.Success(await mapping(result.Value));

        public static async UniTask<Result<TOut>> Map<T, TOut>(this UniTask<Result<T>> resultTask, Func<T, TOut> mapping)
        {
            var result = await resultTask;
            return result.Map(mapping);
        }

        public static Result Bind(this Result result, Func<Result> next) =>
            result.IsFailure ? result : next();

        public static async UniTask<Result> Bind(this Result result, Func<UniTask<Result>> next) =>
            result.IsFailure ? result : await next();

        public static async UniTask<Result> Bind(this UniTask<Result> resultTask, Func<UniTask<Result>> next)
        {
            var result = await resultTask;
            return result.IsFailure ? result : await next();
        }

        public static async UniTask<Result<TOut>> Bind<TOut>(this UniTask<Result> resultTask, Func<UniTask<Result<TOut>>> next)
        {
            var result = await resultTask;
            return result.IsFailure ? Result<TOut>.Failure(result.Error) : await next();
        }

        public static Result<TOut> Bind<T, TOut>(this Result<T> result, Func<T, Result<TOut>> next) =>
            result.IsFailure ? Result<TOut>.Failure(result.Error) : next(result.Value);

        public static async UniTask<Result<TOut>> Bind<T, TOut>(this Result<T> result, Func<T, UniTask<Result<TOut>>> next) =>
            result.IsFailure ? Result<TOut>.Failure(result.Error) : await next(result.Value);

        public static async UniTask<Result<TOut>> Bind<T, TOut>(this UniTask<Result<T>> resultTask, Func<T, Result<TOut>> next)
        {
            var result = await resultTask;
            return result.IsFailure ? Result<TOut>.Failure(result.Error) : next(result.Value);
        }

        public static Result Bind<T>(this Result<T> result, Func<T, Result> next) =>
            result.IsFailure ? Result.Failure(result.Error) : next(result.Value);

        public static async UniTask<Result> Bind<T>(this UniTask<Result<T>> resultTask, Func<T, UniTask<Result>> next)
        {
            var result = await resultTask;
            return result.IsFailure ? Result.Failure(result.Error) : await next(result.Value);
        }

        public static async UniTask<Result<TOut>> Bind<T, TOut>(this UniTask<Result<T>> resultTask, Func<T, UniTask<Result<TOut>>> next)
        {
            var result = await resultTask;
            return result.IsFailure ? Result<TOut>.Failure(result.Error) : await next(result.Value);
        }

        public static Result Tap(this Result result, Action action)
        {
            if (result.IsSuccess)
            {
                action();
            }

            return result;
        }

        public static async UniTask<Result> Tap(this Result result, Func<UniTask> action)
        {
            if (result.IsSuccess)
            {
                await action();
            }

            return result;
        }

        public static Result<T> Tap<T>(this Result<T> result, Action action)
        {
            if (result.IsSuccess)
            {
                action();
            }

            return result;
        }

        public static Result<T> Tap<T>(this Result<T> result, Action<T> action)
        {
            if (result.IsSuccess)
            {
                action(result.Value);
            }

            return result;
        }

        public static async UniTask<Result<T>> Tap<T>(this Result<T> result, Func<T, UniTask> action)
        {
            if (result.IsSuccess)
            {
                await action(result.Value);
            }

            return result;
        }

        public static async UniTask<Result<T>> Tap<T>(this UniTask<Result<T>> resultTask, Action action)
        {
            var result = await resultTask;
            return result.Tap(action);
        }

        public static async UniTask<Result<T>> Tap<T>(this UniTask<Result<T>> resultTask, Action<T> action)
        {
            var result = await resultTask;
            return result.Tap(action);
        }

        public static Result Catch(this Result result, string errorCode, Action<Error> action)
        {
            if (result.IsFailure && result.Error.Code == errorCode)
            {
                action(result.Error);
                return Result.Failure(result.Error.ToHandled());
            }

            return result;
        }

        public static async UniTask<Result> Catch(this UniTask<Result> resultTask, string errorCode, Action<Error> action)
        {
            var result = await resultTask;
            return result.Catch(errorCode, action);
        }

        public static async UniTask<Result> Catch(this UniTask<Result> resultTask, string errorCode, Func<Error, UniTask> action)
        {
            var result = await resultTask;
            if (result.IsFailure && result.Error.Code == errorCode && !result.Error.IsHandled)
            {
                await action(result.Error);
                return Result.Failure(result.Error.ToHandled());
            }

            return result;
        }

        public static Result<T> Catch<T>(this Result<T> result, string errorCode, Action<Error> action)
        {
            if (result.IsFailure && result.Error.Code == errorCode && !result.Error.IsHandled)
            {
                action(result.Error);
                return Result<T>.Failure(result.Error.ToHandled());
            }

            return result;
        }

        public static async UniTask<Result<T>> Catch<T>(this UniTask<Result<T>> resultTask, string errorCode, Action<Error> action)
        {
            var result = await resultTask;
            return result.Catch(errorCode, action);
        }

        public static async UniTask<Result<T>> Catch<T>(this UniTask<Result<T>> resultTask, string errorCode, Func<Error, UniTask> action)
        {
            var result = await resultTask;
            if (result.IsFailure && result.Error.Code == errorCode && !result.Error.IsHandled)
            {
                await action(result.Error);
                return Result<T>.Failure(result.Error.ToHandled());
            }

            return result;
        }

        public static Result Catch(this Result result, ErrorKind errorKind, Action<Error> action)
        {
            if (result.IsFailure && result.Error.Kind == errorKind && !result.Error.IsHandled)
            {
                action(result.Error);
                return Result.Failure(result.Error.ToHandled());
            }

            return result;
        }

        public static async UniTask<Result> Catch(this UniTask<Result> resultTask, ErrorKind errorKind, Action<Error> action)
        {
            var result = await resultTask;
            return result.Catch(errorKind, action);
        }

        public static async UniTask<Result> Catch(this UniTask<Result> resultTask, ErrorKind errorKind, Func<Error, UniTask> action)
        {
            var result = await resultTask;
            if (result.IsFailure && result.Error.Kind == errorKind && !result.Error.IsHandled)
            {
                await action(result.Error);
                return Result.Failure(result.Error.ToHandled());
            }

            return result;
        }

        public static Result<T> Catch<T>(this Result<T> result, ErrorKind errorKind, Action<Error> action)
        {
            if (result.IsFailure && result.Error.Kind == errorKind && !result.Error.IsHandled)
            {
                action(result.Error);
                return Result<T>.Failure(result.Error.ToHandled());
            }

            return result;
        }

        public static async UniTask<Result<T>> Catch<T>(this UniTask<Result<T>> resultTask, ErrorKind errorKind, Action<Error> action)
        {
            var result = await resultTask;
            return result.Catch(errorKind, action);
        }

        public static async UniTask<Result<T>> Catch<T>(this UniTask<Result<T>> resultTask, ErrorKind errorKind, Func<Error, UniTask> action)
        {
            var result = await resultTask;
            if (result.IsFailure && result.Error.Kind == errorKind && !result.Error.IsHandled)
            {
                await action(result.Error);
                return Result<T>.Failure(result.Error.ToHandled());
            }

            return result;
        }

        public static Result<T> Ensure<T>(this Result<T> result, Func<T, bool> predicate, Error validationError)
        {
            if (result.IsFailure) return result;

            return predicate(result.Value) ? result : Result<T>.Failure(validationError);
        }

        public static async UniTask<Result<T>> Ensure<T>(this Result<T> result, Func<T, UniTask<bool>> predicate, Error validationError)
        {
            if (result.IsFailure) return result;

            return await predicate(result.Value) ? result : Result<T>.Failure(validationError);
        }

        public static async UniTask<Result<T>> Ensure<T>(this UniTask<Result<T>> resultTask, Func<T, bool> predicate, Error validationError)
        {
            var result = await resultTask;
            return result.Ensure(predicate, validationError);
        }

        public static Result Recover(this Result result, Action<Error> recoveryAction)
        {
            if (result.IsSuccess) return result;

            recoveryAction(result.Error);
            return Result.Success();
        }

        public static async UniTask<Result> Recover<T>(
            this UniTask<Result> resultTask,
            string errorCode,
            Func<Error, UniTask<Result<T>>> action)
        {
            var result = await resultTask;

            if (result.IsFailure && result.Error.Code == errorCode && !result.Error.IsHandled)
            {
                var recoveryResult = await action(result.Error);

                return recoveryResult.IsSuccess
                    ? Result.Success()
                    : Result.Failure(recoveryResult.Error);
            }

            return result;
        }

        public static async UniTask<Result> Recover(this UniTask<Result> resultTask, string errorCode, Func<Error, UniTask<Result>> action)
        {
            var result = await resultTask;

            if (result.IsFailure && result.Error.Code == errorCode && !result.Error.IsHandled)
            {
                return await action(result.Error);
            }

            return result;
        }

        public static async UniTask<Result> Recover(this Result result, Func<Error, UniTask> recoveryActionAsync)
        {
            if (result.IsSuccess) return result;

            await recoveryActionAsync(result.Error);
            return Result.Success();
        }

        public static Result<T> Recover<T>(this Result<T> result, Func<Error, T> fallback)
        {
            if (result.IsSuccess) return result;

            return Result<T>.Success(fallback(result.Error));
        }

        public static async UniTask<Result<T>> Recover<T>(this Result<T> result, Func<Error, UniTask<T>> fallbackAsync)
        {
            if (result.IsSuccess) return result;

            return Result<T>.Success(await fallbackAsync(result.Error));
        }

        public static TResult Match<TResult>(this Result result, Func<TResult> onSuccess, Func<Error, TResult> onFailure) =>
            result.IsSuccess ? onSuccess() : onFailure(result.Error);

        public static UniTask<TResult> Match<TResult>(this Result result, Func<UniTask<TResult>> onSuccess, Func<Error, UniTask<TResult>> onFailure) =>
            result.IsSuccess ? onSuccess() : onFailure(result.Error);

        public static UniTask<TResult> Match<TResult>(this Result result, Func<UniTask<TResult>> onSuccess, Func<Error, TResult> onFailure) =>
            result.IsSuccess ? onSuccess() : UniTask.FromResult(onFailure(result.Error));

        public static UniTask<TResult> Match<TResult>(this Result result, Func<TResult> onSuccess, Func<Error, UniTask<TResult>> onFailure) =>
            result.IsSuccess ? UniTask.FromResult(onSuccess()) : onFailure(result.Error);

        public static TResult Match<T, TResult>(this Result<T> result, Func<T, TResult> onSuccess, Func<Error, TResult> onFailure) =>
            result.IsSuccess ? onSuccess(result.Value) : onFailure(result.Error);

        public static UniTask<TResult> Match<T, TResult>(this Result<T> result, Func<T, UniTask<TResult>> onSuccess, Func<Error, UniTask<TResult>> onFailure) =>
            result.IsSuccess ? onSuccess(result.Value) : onFailure(result.Error);

        public static async UniTask<TResult> Match<T, TResult>(this UniTask<Result<T>> resultTask, Func<T, UniTask<TResult>> onSuccess, Func<Error, UniTask<TResult>> onFailure)
        {
            var result = await resultTask;
            return await result.Match(onSuccess, onFailure);
        }

        public static async UniTask<TResult> Match<T, TResult>(this UniTask<Result<T>> resultTask, Func<T, TResult> onSuccess, Func<Error, UniTask<TResult>> onFailure)
        {
            var result = await resultTask;
            return await result.Match(onSuccess, onFailure);
        }

        public static UniTask<TResult> Match<T, TResult>(this Result<T> result, Func<T, UniTask<TResult>> onSuccess, Func<Error, TResult> onFailure) =>
            result.IsSuccess ? onSuccess(result.Value) : UniTask.FromResult(onFailure(result.Error));

        public static UniTask<TResult> Match<T, TResult>(this Result<T> result, Func<T, TResult> onSuccess, Func<Error, UniTask<TResult>> onFailure) =>
            result.IsSuccess ? UniTask.FromResult(onSuccess(result.Value)) : onFailure(result.Error);

        public static void Switch(this Result result, Action onSuccess, Action<Error> onFailure)
        {
            if (result.IsSuccess)
            {
                onSuccess();
            }
            else
            {
                onFailure(result.Error);
            }
        }

        public static UniTask Switch(this Result result, Func<UniTask> onSuccess, Func<Error, UniTask> onFailure) =>
            result.IsSuccess ? onSuccess() : onFailure(result.Error);

        public static UniTask Switch(this Result result, Func<UniTask> onSuccess, Action<Error> onFailure)
        {
            if (result.IsSuccess)
            {
                return onSuccess();
            }

            onFailure(result.Error);
            return UniTask.CompletedTask;
        }

        public static UniTask Switch(this Result result, Action onSuccess, Func<Error, UniTask> onFailure)
        {
            if (result.IsSuccess)
            {
                onSuccess();
                return UniTask.CompletedTask;
            }

            return onFailure(result.Error);
        }

        public static void Switch<T>(this Result<T> result, Action<T> onSuccess, Action<Error> onFailure)
        {
            if (result.IsSuccess)
            {
                onSuccess(result.Value);
            }
            else
            {
                onFailure(result.Error);
            }
        }

        public static UniTask Switch<T>(this Result<T> result, Func<T, UniTask> onSuccess, Func<Error, UniTask> onFailure) =>
            result.IsSuccess ? onSuccess(result.Value) : onFailure(result.Error);

        public static UniTask Switch<T>(this Result<T> result, Func<T, UniTask> onSuccess, Action<Error> onFailure)
        {
            if (result.IsSuccess)
            {
                return onSuccess(result.Value);
            }

            onFailure(result.Error);
            return UniTask.CompletedTask;
        }

        public static UniTask Switch<T>(this Result<T> result, Action<T> onSuccess, Func<Error, UniTask> onFailure)
        {
            if (result.IsSuccess)
            {
                onSuccess(result.Value);
                return UniTask.CompletedTask;
            }

            return onFailure(result.Error);
        }

        public static async UniTask Switch<T>(this UniTask<Result<T>> resultTask, Action<T> onSuccess, Action<Error> onFailure)
        {
            var result = await resultTask;
            result.Switch(onSuccess, onFailure);
        }

        public static async UniTask Switch<T>(this UniTask<Result<T>> resultTask, Func<T, UniTask> onSuccess, Action<Error> onFailure)
        {
            var result = await resultTask;
            await result.Switch(onSuccess, onFailure);
        }

        public static async UniTask Switch<T>(this UniTask<Result<T>> resultTask, Action<T> onSuccess, Func<Error, UniTask> onFailure)
        {
            var result = await resultTask;
            await result.Switch(onSuccess, onFailure);
        }

        public static async UniTask Switch<T>(this UniTask<Result<T>> resultTask, Func<T, UniTask> onSuccess, Func<Error, UniTask> onFailure)
        {
            var result = await resultTask;
            await result.Switch(onSuccess, onFailure);
        }

        public static Result<T> Finally<T>(this Result<T> result, Action action)
        {
            action();
            return result;
        }

        public static async UniTask<Result<T>> Finally<T>(this UniTask<Result<T>> resultTask, Action action)
        {
            var result = await resultTask;
            action();
            return result;
        }

        public static Result CatchAll(this Result result, Action<Error> fallbackAction)
        {
            if (result.IsFailure && !result.Error.IsHandled)
            {
                fallbackAction(result.Error);
                return Result.Failure(result.Error.ToHandled());
            }

            return result;
        }

        public static Result<T> CatchAll<T>(this Result<T> result, Action<Error> fallbackAction)
        {
            if (result.IsFailure && !result.Error.IsHandled)
            {
                fallbackAction(result.Error);
                return Result<T>.Failure(result.Error.ToHandled());
            }

            return result;
        }

        public static async UniTask<Result<T>> CatchAll<T>(this UniTask<Result<T>> resultTask, Action<Error> fallbackAction)
        {
            var result = await resultTask;
            return result.CatchAll(fallbackAction);
        }

        public static async UniTask<Result<T>> CatchAll<T>(this UniTask<Result<T>> resultTask, Func<Error, UniTask> fallbackAction)
        {
            var result = await resultTask;
            if (result.IsFailure && !result.Error.IsHandled)
            {
                await fallbackAction(result.Error);
                return Result<T>.Failure(result.Error.ToHandled());
            }

            return result;
        }

        public static async UniTask<Result> CatchAll(this UniTask<Result> resultTask, Func<Error, UniTask> fallbackAction)
        {
            var result = await resultTask;

            if (result.IsFailure && !result.Error.IsHandled)
            {
                await fallbackAction(result.Error);
                return Result.Failure(result.Error.ToHandled());
            }

            return result;
        }
        
        public static async UniTask<Result> CatchAll(this UniTask<Result> resultTask, Action<Error> fallbackAction)
        {
            var result = await resultTask;

            if (result.IsFailure && !result.Error.IsHandled)
            {
                fallbackAction(result.Error);
                
                return Result.Failure(result.Error.ToHandled());
            }

            return result;
        }
        
        public static async UniTask<Result> AsResult<T>(this UniTask<Result<T>> resultTask)
        {
            var result = await resultTask;
            return result.IsSuccess ? Result.Success() : Result.Failure(result.Error);
        }
    }
}