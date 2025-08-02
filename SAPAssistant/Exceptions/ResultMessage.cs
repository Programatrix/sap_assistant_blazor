namespace SAPAssistant.Exceptions
{
    public class ResultMessage
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string ErrorCode { get; set; } = string.Empty; // Opcional, para soporte
        public NotificationType Type { get; set; } = NotificationType.Success;

        public static ResultMessage Ok(string message = null, NotificationType type = NotificationType.Success) => new() { Success = true, Message = message ?? string.Empty, Type = type };
        public static ResultMessage Fail(string message, string errorCode = null, NotificationType type = NotificationType.Error) => new() { Success = false, Message = message, ErrorCode = errorCode ?? string.Empty, Type = type };
    }
    public class ResultMessage<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string ErrorCode { get; set; } = string.Empty;
        public T? Data { get; set; }
        public NotificationType Type { get; set; } = NotificationType.Success;

        public static ResultMessage<T> Ok(T data, string message = "", NotificationType type = NotificationType.Success) => new()
        {
            Success = true,
            Message = message,
            Data = data,
            Type = type
        };

        public static ResultMessage<T> Fail(string message, string errorCode = "", NotificationType type = NotificationType.Error) => new()
        {
            Success = false,
            Message = message,
            ErrorCode = errorCode,
            Data = default,
            Type = type
        };
    }

}
