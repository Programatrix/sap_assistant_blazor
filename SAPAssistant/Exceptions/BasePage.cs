using Microsoft.AspNetCore.Components;

namespace SAPAssistant.Exceptions
{
using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;
    using SAPAssistant.Exceptions;
    using SAPAssistant.Service.Interfaces;

    public abstract class BasePage : ComponentBase
    {
        [Inject] protected INotificationService NotificationService { get; set; } = default!;

        /// Muestra un mensaje de éxito
        protected Task NotifySuccess(string message)
            => NotificationService.NotifySuccess(message);

        /// Muestra un mensaje de error
        protected Task NotifyError(string message)
            => NotificationService.NotifyError(message);

        /// Muestra un mensaje informativo
        protected Task NotifyInfo(string message)
            => NotificationService.NotifyInfo(message);

        /// Muestra un mensaje de advertencia
        protected Task NotifyWarning(string message)
            => NotificationService.NotifyWarning(message);

        /// Maneja una excepción mostrando su mensaje como error
        protected Task HandleError(Exception ex)
            => NotifyError(ex.Message);
    }

}
