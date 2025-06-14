using System.Text.Json;
using System.Text.Json.Serialization;

namespace SAPAssistant.Models
{  

    public class AssistantResponse
    {
        [JsonPropertyName("tipo")]
        public string Tipo { get; set; }

        [JsonPropertyName("mensaje")]
        public string Mensaje { get; set; }

        [JsonPropertyName("tool")]
        public string Tool { get; set; }

        [JsonPropertyName("data")]
        public JsonElement? Data { get; set; }

        [JsonPropertyName("sql")]
        public string? Sql { get; set; }

        [JsonPropertyName("meta")]
        public Dictionary<string, object> Meta { get; set; }
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
