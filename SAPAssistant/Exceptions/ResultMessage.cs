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

}
