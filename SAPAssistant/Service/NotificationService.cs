using System;
using Microsoft.Extensions.Logging;
using SAPAssistant.Exceptions;
using SAPAssistant.Service.Interfaces;
using Microsoft.Extensions.Localization;
using SAPAssistant.Resources;

namespace SAPAssistant.Service;

public class NotificationService : INotificationService
{
    private readonly ILogger<NotificationService> _logger;
    private readonly IStringLocalizer<ErrorMessages> _localizer;

    public NotificationService(ILogger<NotificationService> logger, IStringLocalizer<ErrorMessages> localizer)
    {
        _logger = logger;
        _localizer = localizer;
    }

    // Evento al que se suscriben los componentes que muestran mensajes
    public event Action<ResultMessage>? OnNotify;

    // Método para emitir mensajes de notificación
    public void Notify(ResultMessage message)
    {
        if (!string.IsNullOrEmpty(message.ErrorCode))
        {
            var localized = _localizer[message.ErrorCode];
            if (!localized.ResourceNotFound)
            {
                message.Message = localized;
            }
        }

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

    // Registra un error y dispara una notificación
    public void NotifyException(Exception ex, string context)
    {
        _logger.LogError(ex, "Error en {Context}", context);
        NotifyError($"❌ {context}: {ex.Message}");
    }
}

