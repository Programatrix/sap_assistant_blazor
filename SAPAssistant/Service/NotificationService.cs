using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SAPAssistant.Exceptions;
using SAPAssistant.Service.Interfaces;
using Microsoft.Extensions.Localization;
using SAPAssistant;

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
    public event Func<ResultMessage, Task>? OnNotify;

    // Método para emitir mensajes de notificación
    public async Task Notify(ResultMessage message)
    {
        if (!string.IsNullOrEmpty(message.ErrorCode))
        {
            var localized = _localizer[message.ErrorCode];
            if (!localized.ResourceNotFound)
            {
                message.Message = localized;
            }
        }

        if (OnNotify != null)
        {
            await OnNotify.Invoke(message);
        }
    }

    // Atajo para errores
    public Task NotifyError(string message, string errorCode = "")
    {
        return Notify(ResultMessage.Fail(message, errorCode, NotificationType.Error));
    }

    // Atajo para éxitos
    public Task NotifySuccess(string message)
    {
        return Notify(ResultMessage.Ok(message, NotificationType.Success));
    }

    // Atajo para información
    public Task NotifyInfo(string message)
    {
        return Notify(ResultMessage.Ok(message, NotificationType.Info));
    }

    // Atajo para advertencias
    public Task NotifyWarning(string message)
    {
        return Notify(ResultMessage.Ok(message, NotificationType.Warning));
    }

    // Registra un error y dispara una notificación
    public async Task NotifyException(Exception ex, string context)
    {
        _logger.LogError(ex, "Error en {Context}", context);
        await NotifyError($"❌ {context}: {ex.Message}");
    }
}

