using System;
using System.Threading.Tasks;
using SAPAssistant.Exceptions;

namespace SAPAssistant.Service.Interfaces
{
    public interface INotificationService
    {
        event Func<ServiceResult, Task>? OnNotify;

        Task Notify(ServiceResult message);
        Task NotifyError(string message, string errorCode = "");
        Task NotifySuccess(string message);
        Task NotifyInfo(string message);
        Task NotifyWarning(string message);
        Task NotifyException(Exception ex, string context);
    }
}
