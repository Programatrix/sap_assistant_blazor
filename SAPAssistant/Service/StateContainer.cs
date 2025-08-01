using CommunityToolkit.Mvvm.ComponentModel;
using SAPAssistant.Models;

namespace SAPAssistant.Service
{
    /// <summary>
    /// Contenedor de estado compartido entre componentes y servicios.
    /// </summary>
    public partial class StateContainer : ObservableObject
    {
        [ObservableProperty]
        private LoginResponse? authenticatedUser;

        [ObservableProperty]
        private ConnectionDTO? activeConnection;

        [ObservableProperty]
        private ChatSession? currentChat;
    }
}
