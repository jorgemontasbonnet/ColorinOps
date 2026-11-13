using System;
using System.Threading.Tasks;

namespace Cbsdg.Data.Helpers
{
    public class OpResult<T> : IOpResult
    {
        private readonly T _result = default!;
        public const string NoFoundError = "404";
        public T Result 
        { 
            get 
            {
                if (Success)
                {
                    return _result;
                }
                else
                {
                    throw new InvalidOperationException("Result is not available when operation is not successful");
                }
            }
        }

        public bool Success { get; set; } = true;
        public bool HasError => !Success;
        public string ErrorMessage { get; set; } = string.Empty;
        public string ErrorCode { get; set; } = string.Empty;

        public OpResult()
        {
            
        }
        
        public OpResult(Exception ex)
        {
            Success = false;
            ErrorMessage = ex.Message;
            ErrorCode = ex.GetType().Name;
        }

        public static OpResult<T> FromException(Exception exception)
        {
            return new OpResult<T>(exception);
        }

        public OpResult(T result)
        {
            _result = result;            
        }

        public static OpResult<T> Error(string errorMessage, string errorCode = "") => new() { Success = false, ErrorMessage = errorMessage, ErrorCode = errorCode };

        public async Task UnwrapAsync(Func<T, Task> action, Action<string, string> onError)
        {
            if (Success && action != null)
            {
                await action(Result);
            }
            else
            {
                if (onError != null)
                {
                    onError(ErrorMessage, ErrorCode);
                }
            }
        }

        public void ThrowIfError(string msg)
        {
            if (HasError)
            {
                throw new InvalidOperationException($"{msg}: {ErrorCode} - {ErrorMessage}");
            }
        }

        public void IfOk(Action<T> action)
        {
            if (Success)
            {
                action(Result);
            }
        }

        public async Task IfOkAsync(Func<T, Task> action)
        {
            if (Success)
            {
                await action(Result);
            }
        }
        
        public OpResult<T> Then(Func<T, OpResult<T>> func)
        {
            if (Success)
            {
                return func(Result);
            }
            else
            {
                return this;
            }
        }

        public OpResult<T> Fail(Action<string, string> onError)
        {
            if (HasError)
            {
                onError(ErrorMessage, ErrorCode);
            }
            return this;
        }

        public void Unwrap(Action<T> action, Action<string, string>? onError)
        {
            if (Success && action != null)
            {
                action(Result);
            }
            else
            {
                onError?.Invoke(ErrorMessage, ErrorCode);
            }
        }
    }

    public class OpResultString : OpResult<string>
    {
        public OpResultString() : base()
        {
        }

        public OpResultString(string result) : base(result)
        {
        }
    }

    public class OpResultBool : OpResult<bool>
    {
        public OpResultBool() : base()
        {
        }

        public OpResultBool(bool result) : base(result)
        {
        }
    }

    public class OperationResult : OpResult<object?>
    {
        public OperationResult() : base()
        {
        }

        public OperationResult(object? result) : base(result)
        {
        }

        public static new OperationResult Error(string errorMessage, string errorCode = "") => new OperationResult{ Success = false, ErrorMessage = errorMessage, ErrorCode = errorCode };

        public static new OperationResult FromException(Exception exception)
        {
            return OperationResult.Error(exception.Message, exception.GetType().Name);
        }
    }
}
