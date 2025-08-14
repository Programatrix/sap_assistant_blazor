using CommunityToolkit.Mvvm.ComponentModel;
using SAPAssistant.Models;

namespace SAPAssistant.Service
{
    /// <summary>
    /// Contenedor de estado compartido entre componentes y servicios.
    /// </summary>
    public partial class StateContainer : ObservableObject
    {
        /// <summary>
        /// Conexión activa seleccionada.
        /// </summary>
        [ObservableProperty]
        private ConnectionDTO? activeConnection;

        /// <summary>
        /// Sesión de chat en curso.
        /// </summary>
        [ObservableProperty]
        private ChatSession? currentChat;

        /// <summary>
        /// URL a la que se redirige tras autenticarse.
        /// </summary>
        [ObservableProperty]
        private string? returnUrl;
    }
}
