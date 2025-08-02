using System;
using SAPAssistant.Exceptions;

namespace SAPAssistant.Service.Interfaces
{
    public interface INotificationService
    {
        event Action<ResultMessage>? OnNotify;

        void Notify(ResultMessage message);
        void NotifyError(string message, string errorCode = "");
        void NotifySuccess(string message);
        void NotifyInfo(string message);
        void NotifyWarning(string message);
        void NotifyException(Exception ex, string context);
    }
}
