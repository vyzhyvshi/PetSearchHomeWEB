using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PetSearchHome_WEB.Application.Shared
{
    public class Result
    {
        public bool IsSuccess { get; }
        public string? ErrorMessage { get; }

        protected Result(bool isSuccess, string? errorMessage)
        {
            IsSuccess = isSuccess;
            ErrorMessage = errorMessage;
        }

        public static Result Success() => new Result(true, null);
        public static Result Failure(string errorMessage) => new Result(false, errorMessage);

        public static Result<T> Success<T>(T value) => new Result<T>(true, value, null);
        public static Result<T> Failure<T>(string errorMessage) => new Result<T>(false, default, errorMessage);
    }

    public class Result<T> : Result
    {
        public T? Value { get; }

        internal Result(bool isSuccess, T? value, string? errorMessage)
            : base(isSuccess, errorMessage)
        {
            Value = value;
        }
    }
}