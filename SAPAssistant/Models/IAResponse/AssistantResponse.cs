using System.Text.Json.Serialization;

namespace SAPAssistant.Models
{
    public class AssistantResponse
    {
        [JsonPropertyName("tipo")]
        public string Tipo { get; set; }

        [JsonPropertyName("mensaje")]
        public string Mensaje { get; set; }

        [JsonPropertyName("output")]
        public string Output { get; set; }

        [JsonPropertyName("input")]
        public string Input { get; set; }

        [JsonPropertyName("chat_history")]
        public List<ChatMessage> ChatHistory { get; set; }
    }

    public class ChatMessage
    {
        [JsonPropertyName("content")]
        public string Content { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }  // "human" o "ai"

        [JsonPropertyName("example")]
        public bool Example { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("id")]
        public string Id { get; set; }
    }
}
