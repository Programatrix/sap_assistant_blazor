using Microsoft.AspNetCore.Components;

namespace SAPAssistant.Exceptions
{
    using Microsoft.AspNetCore.Components;
    using SAPAssistant.Exceptions;
    using SAPAssistant.Service;

    public abstract class BasePage : ComponentBase
    {
        [Inject] protected NotificationService NotificationService { get; set; } = default!;

        /// Muestra un mensaje de éxito
        protected void NotifySuccess(string message)
            => NotificationService.NotifySuccess(message);

        /// Muestra un mensaje de error
        protected void NotifyError(string message)
            => NotificationService.NotifyError(message);

        /// Muestra un mensaje informativo
        protected void NotifyInfo(string message)
            => NotificationService.NotifyInfo(message);

        /// Muestra un mensaje de advertencia
        protected void NotifyWarning(string message)
            => NotificationService.NotifyWarning(message);

        /// Maneja una excepción mostrando su mensaje como error
        protected void HandleError(Exception ex)
            => NotifyError(ex.Message);
    }

}
