using System;

namespace Source.Shared
{
    public record Result
    {
        public bool IsSuccess { get; }
        public bool IsFailure => !IsSuccess;

        public Error Error =>
            IsFailure ? _error : throw new InvalidOperationException("Cannot access Error on a successful result.");

        private readonly Error _error;

        private Result(bool isSuccess, Error error)
        {
            IsSuccess = isSuccess;
            _error = error;
        }

        public static Result Success() => new(true, Error.None);
        public static Result Failure(Error error) => new(false, error);

        public static implicit operator Result(Error error) => Failure(error);
    }

    public record Result<T>
    {
        public bool IsSuccess { get; }
        public bool IsFailure => !IsSuccess;

        public T Value =>
            IsSuccess ? _data : throw new InvalidOperationException("Cannot access Value on a failed result.");

        public Error Error =>
            IsFailure ? _error : throw new InvalidOperationException("Cannot access Error on a successful result.");
        
        private readonly T _data;
        
        private readonly Error _error;

        private Result(bool isSuccess, T data, Error error)
        {
            IsSuccess = isSuccess;
            _data = data;
            _error = error;
        }

        public static Result<T> Success(T data) => new(true, data, Error.None);
        public static Result<T> Failure(Error error) => new(false, default, error);

        public static implicit operator Result<T>(T data) => Success(data);

        public static implicit operator Result<T>(Error error) => Failure(error);

        public static implicit operator Result(Result<T> result)
        {
            return result.IsSuccess
                ? Result.Success()
                : Result.Failure(result.Error);
        }
    }
}