using System;
using SAPAssistant.Models;

namespace SAPAssistant.Service
{
    /// <summary>
    /// Contenedor de estado compartido entre componentes y servicios.
    /// </summary>
    public class StateContainer
    {
        public LoginResponse? AuthenticatedUser { get; private set; }
        public ConnectionDTO? ActiveConnection { get; private set; }
        public ChatSession? CurrentChat { get; private set; }

        public event Action? OnChange;

        public void SetUser(LoginResponse? user)
        {
            AuthenticatedUser = user;
            NotifyStateChanged();
        }

        public void SetConnection(ConnectionDTO? connection)
        {
            ActiveConnection = connection;
            NotifyStateChanged();
        }

        public void SetChat(ChatSession? chat)
        {
            CurrentChat = chat;
            NotifyStateChanged();
        }

        private void NotifyStateChanged() => OnChange?.Invoke();
    }
}
