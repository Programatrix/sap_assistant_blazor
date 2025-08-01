using System;

namespace SAPAssistant.Exceptions
{
    public class DashboardServiceException : Exception
    {
        public DashboardServiceException() { }

        public DashboardServiceException(string message) : base(message) { }

        public DashboardServiceException(string message, Exception innerException) : base(message, innerException) { }
    }
}
