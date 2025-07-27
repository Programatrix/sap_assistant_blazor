using System.Text.Json;
using System.Text.Json.Serialization;

namespace SAPAssistant.Models
{
    public class AssistantResponse
    {
        [JsonPropertyName("tipo")]
        public string Tipo { get; set; } = "system";

        [JsonPropertyName("role")]
        public string? Role { get; set; }

        [JsonPropertyName("mensaje")]
        public string? Mensaje { get; set; }

        [JsonPropertyName("tool")]
        public string? Tool { get; set; }

        [JsonPropertyName("data")]
        public JsonElement? Data { get; set; }

        [JsonPropertyName("sql")]
        public string? Sql { get; set; }

        [JsonPropertyName("meta")]
        public Dictionary<string, object>? Meta { get; set; }
    }
}
