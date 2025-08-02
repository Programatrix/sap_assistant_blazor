namespace SAPAssistant.Exceptions
{
    public class OperationResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string ErrorCode { get; set; } = string.Empty;

        public static OperationResult Ok(string message = "") => new()
        {
            Success = true,
            Message = message
        };

        public static OperationResult Fail(string message, string errorCode = "") => new()
        {
            Success = false,
            Message = message,
            ErrorCode = errorCode
        };
    }

    public class OperationResult<T> : OperationResult
    {
        public T? Data { get; set; }

        public static OperationResult<T> Ok(T data, string message = "") => new()
        {
            Success = true,
            Message = message,
            Data = data
        };

        public static new OperationResult<T> Fail(string message, string errorCode = "") => new()
        {
            Success = false,
            Message = message,
            ErrorCode = errorCode,
            Data = default
        };
    }
}
