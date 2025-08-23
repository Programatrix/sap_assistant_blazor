using SAPAssistant.Models.Chat;
using System.Text.Json.Serialization;

namespace SAPAssistant.Models
{
    public class ChatSessionResult
    {
        public bool success { get; set; }
        public List<ChatSession> data { get; set; } = new();
    }

    public class ChatSessionSingleResult
    {
        public bool success { get; set; }
        public ChatSession data { get; set; } = new();
    }
    public class ChatSession
    {
        public string Id { get; set; }
        public DateTime Fecha { get; set; }
        public string Titulo { get; set; } = "Nuevo Chat";
        public List<Dictionary<string, object>> MensajesRaw { get; set; } = new(); // del backend

        [JsonPropertyName("messages")]
        public List<AssistantResponse> Messages { get; set; } = new(); // lo que usas en UI
    }
       

}
