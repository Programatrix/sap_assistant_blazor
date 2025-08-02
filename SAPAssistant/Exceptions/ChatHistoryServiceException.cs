using System;

namespace SAPAssistant.Exceptions
{
    public class ChatHistoryServiceException : Exception
    {
        public ChatHistoryServiceException() { }
        public ChatHistoryServiceException(string message) : base(message) { }
        public ChatHistoryServiceException(string message, Exception innerException) : base(message, innerException) { }
    }
}
