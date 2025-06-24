using SAPAssistant.Models.Chat;

namespace SAPAssistant.Models
{
    public class ChatHistory
    {
        public string Usuario { get; set; }
        public List<ChatSession> HistoricoChats { get; set; } = new();
    }

    public class ChatSession
    {
        public string Id { get; set; }
        public DateTime Fecha { get; set; }
        public string Titulo { get; set; } = "Nuevo Chat";
        public List<Dictionary<string, object>> MensajesRaw { get; set; } = new(); // del backend
        public List<MessageBase> Mensajes { get; set; } = new(); // lo que usas en UI
    }

    public class Message
    {
        public string Content { get; set; } = string.Empty;
        public bool IsUser { get; set; }
    }

}
