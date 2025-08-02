using System;
using SAPAssistant.Exceptions;

namespace SAPAssistant.Service
{
    public class NotificationService
    {
        // Evento al que se suscriben los componentes que muestran mensajes
        public event Action<ResultMessage>? OnNotify;

        // Método para emitir mensajes de notificación
        public void Notify(ResultMessage message)
        {
            OnNotify?.Invoke(message);
        }

        // Atajo para errores
        public void NotifyError(string message, string errorCode = "")
        {
            Notify(ResultMessage.Fail(message, errorCode, NotificationType.Error));
        }

        // Atajo para éxitos
        public void NotifySuccess(string message)
        {
            Notify(ResultMessage.Ok(message, NotificationType.Success));
        }

        // Atajo para información
        public void NotifyInfo(string message)
        {
            Notify(ResultMessage.Ok(message, NotificationType.Info));
        }

        // Atajo para advertencias
        public void NotifyWarning(string message)
        {
            Notify(ResultMessage.Ok(message, NotificationType.Warning));
        }
    }

}
