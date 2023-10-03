using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TechChallenge.Domain.Core.Primitives
{
    public class Result
    {
        #region Properties

        public bool IsSuccess { get; }
        public bool IsFailure => !IsSuccess;
        public Error Error { get; }

        #endregion

        #region Constructors

        protected Result(bool isSuccess, Error error)
        {
            if (isSuccess && error != Error.None)
                throw new InvalidOperationException();

            if (!isSuccess && error == Error.None)
                throw new InvalidOperationException();

            IsSuccess = isSuccess;
            Error = error;
        }

        #endregion

        #region Factory Methods

        public static Result Success()
            => new Result(true, Error.None);

        public static Result<TValue> Success<TValue>(TValue value)
            => new Result<TValue>(value, true, Error.None);

        public static Result<TValue> Create<TValue>(TValue value, Error error)
            where TValue : class
            => value is null ? Failure<TValue>(error) : Success(value);

        public static Result Failure(Error error)
            => new Result(false, error);

        public static Result<TValue> Failure<TValue>(Error error)
            => new Result<TValue>(default!, false, error);

        public static Result FirstFailureOrSuccess(params Result[] results)
        {
            foreach (Result result in results)
            {
                if (result.IsFailure)
                    return result;
            }

            return Success();
        }

        #endregion
    }

    public class Result<TValue> : Result
    {
        #region Read-Only Fields

        private readonly TValue _value;

        #endregion

        #region Properties

        public TValue Value => IsSuccess
            ? _value
            : throw new InvalidOperationException("The value of a failure result can not be accessed.");

        #endregion

        #region Constructors

        protected internal Result(TValue value, bool isSuccess, Error error)
            : base(isSuccess, error)
            => _value = value;

        #endregion

        #region Operators

        public static implicit operator Result<TValue>(TValue value)
            => Success(value);

        #endregion
    }

    public static class ResultExtensions
    {
        #region Extension Methods

        public static Result<TValue> Ensure<TValue>(this Result<TValue> result, Func<TValue, bool> predicate, Error error)
        {
            if (result.IsFailure)
                return result;

            return result.IsSuccess && predicate(result.Value) ? result : Result.Failure<TValue>(error);
        }

        public static Result<TOutput> Map<TInput, TOutput>(this Result<TInput> result, Func<TInput, TOutput> func)
            => result.IsSuccess ? func(result.Value) : Result.Failure<TOutput>(result.Error);

        public static async Task<Result> Bind<TInput>(this Result<TInput> result, Func<TInput, Task<Result>> func)
            => result.IsSuccess ? await func(result.Value) : Result.Failure(result.Error);

        public static async Task<Result<TOutput>> Bind<TInput, TOutput>(this Result<TInput> result, Func<TInput, Task<Result<TOutput>>> func)
            => result.IsSuccess ? await func(result.Value) : Result.Failure<TOutput>(result.Error);

        public static async Task<TValue> Match<TValue>(this Task<Result> resultTask, Func<TValue> onSuccess, Func<Error, TValue> onFailure)
        {
            Result result = await resultTask;
            return result.IsSuccess ? onSuccess() : onFailure(result.Error);
        }

        public static async Task<TOutput> Match<TInput, TOutput>(this Task<Result<TInput>> resultTask, Func<TInput, TOutput> onSuccess, Func<Error, TOutput> onFailure)
        {
            Result<TInput> result = await resultTask;
            return result.IsSuccess ? onSuccess(result.Value) : onFailure(result.Error);
        }

        #endregion
    }
}
