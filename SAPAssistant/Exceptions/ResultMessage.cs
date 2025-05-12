namespace SAPAssistant.Exceptions
{
    public class ResultMessage
    {
        public bool Success { get; set; }
        public string Message { get; set; } = "";
        public string ErrorCode { get; set; } = ""; // Opcional, para soporte

        public static ResultMessage Ok(string message = null) => new() { Success = true, Message = message };
        public static ResultMessage Fail(string message, string errorCode = null) => new() { Success = false, Message = message, ErrorCode = errorCode };
    }
    public class ResultMessage<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = "";
        public string ErrorCode { get; set; } = "";
        public T? Data { get; set; }

        public static ResultMessage<T> Ok(T data, string message = "") => new()
        {
            Success = true,
            Message = message,
            Data = data
        };

        public static ResultMessage<T> Fail(string message, string errorCode = "") => new()
        {
            Success = false,
            Message = message,
            ErrorCode = errorCode,
            Data = default
        };
    }

}
